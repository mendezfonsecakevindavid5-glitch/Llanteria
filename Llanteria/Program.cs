using Llanteria.Data;
using Llanteria.Infrastructure;
using Llanteria.Seguridad;
using Llanteria.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

// Formato colombiano para fechas y moneda (la lectura de decimales se hace con cultura invariante, ver InvariantDecimalModelBinder).
var cultura = new CultureInfo("es-CO");
CultureInfo.DefaultThreadCurrentCulture = cultura;
CultureInfo.DefaultThreadCurrentUICulture = cultura;

var builder = WebApplication.CreateBuilder(args);

// Configuración privada: appsettings.json ya no está dentro del proyecto. Vive en la carpeta "Llanteria_Privado"
// (junto a la carpeta del proyecto, fuera de lo que se publica) o en la ruta de la variable LLANTERIA_CONFIG_DIR.
// Las variables de entorno siguen teniendo prioridad sobre el archivo.
var carpetaPrivada = Environment.GetEnvironmentVariable("LLANTERIA_CONFIG_DIR")
    ?? Path.GetFullPath(Path.Combine(builder.Environment.ContentRootPath, "..", "Llanteria_Privado"));
var archivoPrivado = Path.Combine(carpetaPrivada, "appsettings.json");
if (!File.Exists(archivoPrivado) && string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("ConnectionStrings__Con")))
    throw new InvalidOperationException(
        $"No se encontró la configuración privada en \"{archivoPrivado}\". Crea esa carpeta con appsettings.json " +
        "o define la variable de entorno LLANTERIA_CONFIG_DIR.");
if (Directory.Exists(carpetaPrivada))
    builder.Configuration.AddJsonFile(new Microsoft.Extensions.FileProviders.PhysicalFileProvider(carpetaPrivada), "appsettings.json", optional: true, reloadOnChange: false);
builder.Configuration.AddEnvironmentVariables(); // re-agregado al final para que siga ganando sobre el archivo privado

// Servicio de cifrado de datos (AES-256-GCM). La clave sale de la configuración privada, nunca del código.
ServicioCifrado.Inicializar(new ServicioCifrado(builder.Configuration["Cifrado:Clave"] ?? ""));

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Home/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
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
        "Falta la cadena de conexión \"ConnectionStrings:Con\". Agrégala en Llanteria_Privado/appsettings.json (o en una variable de entorno).");

builder.Services.AddDbContext<LlanteriaDbContext>(options => options.UseSqlServer(cadenaConexion));

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

// Cada servicio se registra una sola vez (antes EmailService y IPerfilService estaban duplicados).
builder.Services.AddSingleton<EmailService>();
builder.Services.AddSingleton(builder.Configuration.GetSection("Negocio").Get<NegocioInfo>() ?? new NegocioInfo());
builder.Services.AddSingleton(builder.Configuration.GetSection("Pagos").Get<PagoInfo>() ?? new PagoInfo());
builder.Services.AddHostedService<RecordatorioCitasService>();

Rotativa.AspNetCore.RotativaConfiguration.Setup(builder.Environment.WebRootPath);

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(10);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<LlanteriaDbContext>();
        db.Database.EnsureCreated();
        EsquemaUpdater.Aplicar(db);
        DatosIniciales.Asegurar(db);
        if (builder.Configuration.GetValue<bool>("Cifrado:MigrarDatosExistentes"))
        {
            var n = ProtegerDatos.Ejecutar(db);
            scope.ServiceProvider.GetRequiredService<ILogger<Program>>()
                .LogInformation("Cifrado de datos existentes: {N} valores protegidos. Pon Cifrado:MigrarDatosExistentes en false.", n);
        }
    }
    catch (Exception ex)
    {
        scope.ServiceProvider.GetRequiredService<ILogger<Program>>()
            .LogError(ex, "No se pudo verificar la base de datos al iniciar.");
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles(); // también sirve los archivos subidos en tiempo de ejecución (productos, logos, soportes)
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();
app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
