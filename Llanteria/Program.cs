using Llanteria.Data;
using Llanteria.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<LlanteriaDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Con")));


builder.Services.AddScoped<TipoDocumentoService>();
builder.Services.AddScoped<SexoService>();
builder.Services.AddScoped<UsuarioService>();
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

// Cambia tu línea actual por esta:
Rotativa.AspNetCore.RotativaConfiguration.Setup(builder.Environment.WebRootPath);

var app = builder.Build();


using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<LlanteriaDbContext>();

        // Crea la base de datos y todas sus tablas automáticamente si no existen
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
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();