using Llanteria.Data;
using Llanteria.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies; // Necesario para la autenticación

var builder = WebApplication.CreateBuilder(args);

// 1. Configuración de Autenticación por Cookies
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login"; // Ruta de tu controlador de Login
        options.AccessDeniedPath = "/Home/AccessDenied";
    });

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<LlanteriaDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Con")));

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
builder.Services.AddScoped<Llanteria.Services.IPerfilService, Llanteria.Services.PerfilService>();

Rotativa.AspNetCore.RotativaConfiguration.Setup(builder.Environment.WebRootPath);


Rotativa.AspNetCore.RotativaConfiguration.Setup(builder.Environment.WebRootPath);

builder.Services.AddSession(options => {
    options.IdleTimeout = TimeSpan.FromMinutes(10); // El código expira en 10 min
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<LlanteriaDbContext>();
        context.Database.EnsureCreated();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Ocurrió un error al crear la Base de Datos automáticamente.");
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseSession();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();