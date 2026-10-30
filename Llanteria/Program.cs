using Llanteria.Data;
using Llanteria.Infrastructure;
using Llanteria.Seguridad;
using Llanteria.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.ResponseCompression;
using System.Globalization;
using System.IO.Compression;

// Administración de la bóveda de secretos: "dotnet run -- boveda ..." (ver Seguridad/BovedaCli.cs). No arranca el sitio.
if (args.Length > 0 && string.Equals(args[0], "boveda", StringComparison.OrdinalIgnoreCase))
    Environment.Exit(BovedaCli.Ejecutar(args.Skip(1).ToArray()));

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

// Bóveda de secretos: abre Privado/boveda.json (cifrada) y comprueba que appsettings no fue editado a mano. Si el sello no coincide, la
// aplicación no arranca. Sin bóveda (todavía no corriste "boveda init") todo funciona como antes, leyendo appsettings.json.
var secretosBoveda = Boveda.CargarParaApp();

// reloadOnChange:false -> un cambio en el archivo con la aplicación encendida no tiene efecto (ni siquiera uno malicioso).
builder.Configuration.AddJsonFile(appsettingsPrivado, optional: false, reloadOnChange: false);
builder.Configuration.AddJsonFile(CarpetaPrivada.Archivo($"appsettings.{builder.Environment.EnvironmentName}.json"), optional: true, reloadOnChange: false);
if (secretosBoveda != null) builder.Configuration.AddInMemoryCollection(secretosBoveda); // los secretos de la bóveda mandan sobre los archivos

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
builder.Services.AddHttpClient();
builder.Services.AddSingleton<PowerBiService>(); // panel interactivo con Power BI

// Rotativa busca wkhtmltopdf.exe dentro de wwwroot/<relativePath>. Los binarios están en wwwroot/Rotativa/Rotativa,
// así que se detecta la carpeta que realmente contiene el .exe (si estuviera en wwwroot/Rotativa también funciona).
var rutaRotativa = new[] { "Rotativa", Path.Combine("Rotativa", "Rotativa") }
    .FirstOrDefault(r => File.Exists(Path.Combine(builder.Environment.WebRootPath, r, "wkhtmltopdf.exe"))) ?? "Rotativa";
Rotativa.AspNetCore.RotativaConfiguration.Setup(builder.Environment.WebRootPath, rutaRotativa);

builder.Services.AddMemoryCache(); // catálogo: lista de productos de cada categoría guardada unos segundos (ver CatalogoControllerBase)

// Compresión: las páginas y los scripts viajan 70-80 % más livianos, lo que más se nota con datos móviles o señal débil.
builder.Services.AddResponseCompression(o =>
{
    o.EnableForHttps = true;
    o.Providers.Add<BrotliCompressionProvider>();
    o.Providers.Add<GzipCompressionProvider>();
    o.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[] { "image/svg+xml", "application/manifest+json" });
});
builder.Services.Configure<BrotliCompressionProviderOptions>(o => o.Level = CompressionLevel.Fastest);
builder.Services.Configure<GzipCompressionProviderOptions>(o => o.Level = CompressionLevel.Fastest);

builder.Services.AddDistributedMemoryCache(); // almacenamiento de la sesión (guarda los códigos de verificación mientras se escriben)
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
    Paso("datos iniciales (roles y catálogos)", () => UsuariosIniciales.AsegurarCatalogos(db));
    Paso("usuarios iniciales (administrador, empleado y usuario)", () => UsuariosIniciales.Asegurar(db, log));
    // Datos de ejemplo (INSERT INTO incluidos en el proyecto): solo en una base recién creada. Va antes de LlanteriaInicial
    // (para que lo que quede sin sede se asigne a la primera llantería) y antes del cifrado (para cifrar lo recién insertado).
    Paso("datos de ejemplo (Scripts/DatosEjemploInicial.sql)", () => DatosEjemplo.Asegurar(db, log, app.Configuration, app.Environment));
    Paso("llantería inicial", () => LlanteriaInicial.Asegurar(db, scope.ServiceProvider.GetRequiredService<NegocioInfo>()));
    Paso("cifrado de datos existentes", () => CifradoMigrador.Aplicar(db, log));
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseResponseCompression();

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

// También sirve los archivos subidos en tiempo de ejecución (productos, logos, soportes).
// Caché del navegador: lo que ya se descargó no se vuelve a pedir (clave con conexión lenta o intermitente).
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        var ruta = ctx.Context.Request.Path.Value ?? "";
        var cabeceras = ctx.Context.Response.Headers;
        if (ruta.Equals("/sw.js", StringComparison.OrdinalIgnoreCase))
            cabeceras.CacheControl = "no-cache"; // el service worker siempre se revalida para que las actualizaciones lleguen
        else if (ctx.Context.Request.Query.ContainsKey("v") || ruta.StartsWith("/lib/", StringComparison.OrdinalIgnoreCase))
            cabeceras.CacheControl = "public,max-age=31536000,immutable"; // css/js con ?v=hash (asp-append-version) y librerías
        else if (ruta.StartsWith("/images/", StringComparison.OrdinalIgnoreCase))
            cabeceras.CacheControl = "public,max-age=86400"; // imágenes: un día (si cambias una foto, se ve al día siguiente como máximo)
    }
});
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