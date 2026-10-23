using Llanteria.Data;
using Llanteria.Infrastructure;
using Llanteria.Seguridad;
using Llanteria.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

// Formato colombiano para fechas y moneda (la lectura de decimales se hace con cultura invariante, ver InvariantDecimalModelBinder).
var cultura = new CultureInfo("es-CO");
CultureInfo.DefaultThreadCurrentCulture = cultura;
CultureInfo.DefaultThreadCurrentUICulture = cultura;

var builder = WebApplication.CreateBuilder(args);

// ---- Carpeta privada: appsettings.json, clave maestra de cifrado y llaves de las cookies (ver Infrastructure/CarpetaPrivada.cs) ----
var carpetaPrivada = CarpetaPrivada.Resolver(builder.Environment.ContentRootPath);
var appsettingsPrivado = CarpetaPrivada.Archivo("appsettings.json");

if (!File.Exists(appsettingsPrivado))
    throw new FileNotFoundException(
        $"No se encontró appsettings.json en la carpeta privada ({carpetaPrivada}). Cópialo allí (hay una plantilla en Privado/appsettings.ejemplo.json) " +
        $"o define la variable de entorno {CarpetaPrivada.VariableEntorno} con la ruta correcta.");

builder.Configuration.AddJsonFile(appsettingsPrivado, optional: false, reloadOnChange: true);
builder.Configuration.AddJsonFile(CarpetaPrivada.Archivo($"appsettings.{builder.Environment.EnvironmentName}.json"), optional: true, reloadOnChange: true);

// Las variables de entorno y la línea de comandos siguen teniendo prioridad sobre los archivos.
builder.Configuration.AddEnvironmentVariables();
builder.Configuration.AddCommandLine(args);

// 🔐 Inicialización correcta del cifrado autenticado de la aplicación (AES-256-GCM)
ServicioCifrado.Inicializar(new ServicioCifrado(builder.Configuration["Cifrado:Clave"] ?? ""));

// Las cookies de sesión y los tokens antiforgery se protegen con llaves guardadas en la carpeta privada (y con DPAPI en Windows).
var proteccion = builder.Services.AddDataProtection()
    .SetApplicationName("Llanteria")
    .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(carpetaPrivada, "llaves")));

if (OperatingSystem.IsWindows())
    proteccion.ProtectKeysWithDpapi();

builder.WebHost.ConfigureKestrel(o => o.AddServerHeader = false);

var politicaCookieSegura = builder.Environment.IsDevelopment() ? CookieSecurePolicy.SameAsRequest : CookieSecurePolicy.Always;

builder.Services.AddAntiforgery(o =>
{
    o.Cookie.HttpOnly = true;
    o.Cookie.SecurePolicy = politicaCookieSegura;
    o.Cookie.SameSite = SameSiteMode.Strict;
    // Sin esto, [ValidateAntiForgeryToken] solo lee el token de un formulario (application/x-www-form-urlencoded):
    // cualquier fetch que mande JSON (Registro, Reestablecer contraseña) fallaba siempre con 400, aunque mandara
    // el token en un encabezado, porque el servidor nunca miraba ese encabezado.
    o.HeaderName = "RequestVerificationToken";
});

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Home/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = politicaCookieSegura;
        options.Cookie.SameSite = SameSiteMode.Lax;
    });

builder.Services.AddControllersWithViews(options =>
{
    // Las propiedades de navegación (IdClienteNavigation, etc.) no vienen del formulario.
    options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
    options.ModelBinderProviders.Insert(0, new InvariantDecimalModelBinderProvider());
    options.Conventions.Add(new RoleConvention());
    options.Filters.Add<DbUpdateExceptionFilter>();
});

var cadenaConexion = builder.Configuration.GetConnectionString("Con");
if (string.IsNullOrWhiteSpace(cadenaConexion))
    throw new InvalidOperationException(
        "Falta la cadena de conexión \"ConnectionStrings:Con\". Agrégala en Privado/appsettings.json (o en User Secrets / variables de entorno).");

builder.Services.AddDbContext<LlanteriaDbContext>(options => options.UseSqlServer(cadenaConexion));

builder.Services.AddSingleton<Microsoft.AspNetCore.Authorization.IAuthorizationHandler, AdministradorAccesoTotal>(); // el administrador entra a todo el panel

builder.Services.AddScoped<TipoDocumentoService>();
builder.Services.AddScoped<SexoService>();
builder.Services.AddScoped<UsuarioService>();
builder.Services.AddScoped<ILogService, LogService>();
builder.Services.AddScoped<ProveedoreService>();
builder.Services.AddScoped<ClienteService>();
builder.Services.AddScoped<RoleService>();
builder.Services.AddScoped<EmpleadoService>();
builder.Services.AddScoped<ProductoService>();
builder.Services.AddScoped<FacturaService>();
builder.Services.AddScoped<VehiculoService>();
builder.Services.AddScoped<GastoService>();
builder.Services.AddScoped<MarcaService>();
builder.Services.AddScoped<BodegaService>();
builder.Services.AddScoped<TipoServicioService>();
builder.Services.AddScoped<CatalogoIncentivoService>();
builder.Services.AddScoped<IPerfilService, PerfilService>();
builder.Services.AddScoped<PaisService>();
builder.Services.AddScoped<CredentialGenerationService>();
builder.Services.AddScoped<AlmacenImagenes>();

// Cada servicio se registra una sola vez
builder.Services.AddSingleton<EmailService>();
builder.Services.AddSingleton(builder.Configuration.GetSection("Negocio").Get<NegocioInfo>() ?? new NegocioInfo());
builder.Services.AddSingleton(builder.Configuration.GetSection("Pagos").Get<PagoInfo>() ?? new PagoInfo());
builder.Services.AddHostedService<RecordatorioCitasService>();

// Rotativa busca wkhtmltopdf.exe dentro de wwwroot/<relativePath>. Los binarios están en wwwroot/Rotativa/Rotativa,
// así que se detecta la carpeta que realmente contiene el .exe (si estuviera en wwwroot/Rotativa también funciona).
var rutaRotativa = new[] { "Rotativa", Path.Combine("Rotativa", "Rotativa") }
    .FirstOrDefault(r => File.Exists(Path.Combine(builder.Environment.WebRootPath, r, "wkhtmltopdf.exe"))) ?? "Rotativa";
Rotativa.AspNetCore.RotativaConfiguration.Setup(builder.Environment.WebRootPath, rutaRotativa);

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(10);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy = politicaCookieSegura;
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    // Cada paso va en su propio bloque: si uno falla (por ejemplo, un ajuste de esquema), los demás se ejecutan igual
    // y el motivo queda en el registro. Antes un solo fallo dejaba sin roles ni catálogos y el registro de usuarios fallaba.
    var log = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    var db = scope.ServiceProvider.GetRequiredService<LlanteriaDbContext>();

    void Paso(string nombre, Action accion)
    {
        try { accion(); }
        catch (Exception ex) { log.LogError(ex, "Fallo al iniciar la base de datos en el paso: {Paso}.", nombre); }
    }

    Paso("crear la base de datos", () => db.Database.EnsureCreated());
    Paso("actualizar el esquema", () => EsquemaUpdater.Aplicar(db));
    Paso("datos iniciales (roles y catálogos)", () => DatosIniciales.Asegurar(db));
    Paso("llantería inicial", () => LlanteriaInicial.Asegurar(db, scope.ServiceProvider.GetRequiredService<NegocioInfo>()));
    Paso("usuarios iniciales (administrador, empleado y usuario)", () => UsuariosIniciales.Asegurar(db, log));
    Paso("cifrado de datos existentes", () => CifradoMigrador.Aplicar(db, log));
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

// Cabeceras de seguridad: evitan que el sitio se incruste en otro, que el navegador "adivine" tipos de archivo
// y limitan lo que la página puede pedir del dispositivo.
app.Use(async (ctx, next) =>
{
    var h = ctx.Response.Headers;
    h["X-Content-Type-Options"] = "nosniff";
    h["X-Frame-Options"] = "SAMEORIGIN";
    h["Referrer-Policy"] = "strict-origin-when-cross-origin";
    h["Permissions-Policy"] = "camera=(), microphone=(), payment=()";
    h["Cross-Origin-Opener-Policy"] = "same-origin";
    await next();
});

app.UseStaticFiles(); // también sirve los archivos subidos en tiempo de ejecución (productos, logos, soportes)
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseRolVisitante(); // todo visitante sin sesión recibe el rol "Usuario no registrado"

// Las páginas de quien tiene sesión no se guardan en caché: al cerrar sesión, "Atrás" no muestra datos privados.
app.Use(async (ctx, next) =>
{
    if (ctx.User.Identity?.IsAuthenticated == true)
        ctx.Response.Headers.CacheControl = "no-store";
    await next();
});

app.UseAuthorization();

app.MapStaticAssets();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();