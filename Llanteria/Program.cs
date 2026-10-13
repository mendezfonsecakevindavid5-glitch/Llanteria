 using Llanteria.Data;
using Llanteria.Infrastructure;
using Llanteria.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

// Formato colombiano para fechas y moneda (la lectura de decimales se hace con cultura invariante, ver InvariantDecimalModelBinder).
var cultura = new CultureInfo("es-CO");
CultureInfo.DefaultThreadCurrentCulture = cultura;
CultureInfo.DefaultThreadCurrentUICulture = cultura;

var builder = WebApplication.CreateBuilder(args);

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
    // Sin esto, ModelState.IsValid era siempre falso en Cliente, Factura, Vehículo, Usuario...
    options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
    options.ModelBinderProviders.Insert(0, new InvariantDecimalModelBinderProvider());
    options.Conventions.Add(new RoleConvention());
    options.Filters.Add<DbUpdateExceptionFilter>();
});

var cadenaConexion = builder.Configuration.GetConnectionString("Con");
if (string.IsNullOrWhiteSpace(cadenaConexion))
    throw new InvalidOperationException(
        "Falta la cadena de conexión \"ConnectionStrings:Con\". Agrégala en appsettings.json (o en User Secrets); mira appsettings.EJEMPLO.json.");

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
builder.Services.AddSingleton<EmailService>();
builder.Services.AddSingleton(builder.Configuration.GetSection("Negocio").Get<NegocioInfo>() ?? new NegocioInfo());
builder.Services.AddHostedService<RecordatorioCitasService>();
builder.Services.AddScoped<IPerfilService, PerfilService>();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(10);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

Rotativa.AspNetCore.RotativaConfiguration.Setup(builder.Environment.WebRootPath);

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<LlanteriaDbContext>();
        db.Database.EnsureCreated();
        EsquemaUpdater.Aplicar(db);
        DatosIniciales.Asegurar(db);
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
app.UseRouting();
app.UseSession();
app.UseAuthentication();   // Faltaba: sin esto User.Identity nunca estaba autenticado
app.UseAuthorization();

app.UseStaticFiles(); // sirve también los archivos subidos en tiempo de ejecución (productos, logos, soportes)
app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
