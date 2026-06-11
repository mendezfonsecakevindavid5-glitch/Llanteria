using Microsoft.AspNetCore.Mvc;
using Llanteria.Models;
using Llanteria.Services;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Llanteria.Controllers;

public class ProductoController : Controller
{
    private readonly ProductoService ser;
    private readonly ProveedoreService provSer; // Para el combo de proveedores
    private readonly MarcaService marcSer; // Para el combo de marcas

    public ProductoController(ProductoService productoService, ProveedoreService proveedoreService, MarcaService marcaService)
    {
        ser = productoService;
        provSer = proveedoreService;
        marcSer = marcaService;
    }

    // Listado de productos
    public ActionResult Index()
    {
        var lista = ser.GetProductos();
        return View(lista);
    }

    // ACCIÓN NUEVA: Filtrar por medidas desde la Home
    public IActionResult FiltrarMedidas(string ancho, string perfil, string diametro)
    {
        // Llamamos al servicio para obtener los productos que coincidan con las medidas
        // Si no tienes este método en el servicio, podemos filtrar la lista aquí
        var resultados = ser.GetProductos().Where(p =>
            (string.IsNullOrEmpty(ancho) || p.DetalleProducto?.Ancho == ancho) &&
            (string.IsNullOrEmpty(perfil) || p.DetalleProducto?.Perfil == perfil) &&
            (string.IsNullOrEmpty(diametro) || p.DetalleProducto?.Diametro == diametro)
        ).ToList();

        ViewBag.FiltroActivo = true;
        return View("Index", resultados);
    }

    // Vista para crear nuevo producto
    public ActionResult Create()
    {
        CargarCombos();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult Create(Producto p)
    {
        try
        {
            if (ModelState.IsValid)
            {
                ser.AddProducto(p);
                return RedirectToAction(nameof(Index));
            }
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", "Error al guardar: " + ex.Message);
        }
        CargarCombos();
        return View(p);
    }

    // Vista para editar producto
    public ActionResult Edit(int id)
    {
        var p = ser.GetProducto(id);
        if (p == null) return NotFound();

        CargarCombos();
        return View(p);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult Edit(int id, Producto ob)
    {
        try
        {
            if (ModelState.IsValid)
            {
                ser.UpdateProducto(ob);
                return RedirectToAction(nameof(Index));
            }
        }
        catch
        {
            ModelState.AddModelError("", "Error al actualizar.");
        }
        CargarCombos();
        return View(ob);
    }

    // Acción para eliminar
    public ActionResult Delete(int id)
    {
        ser.DeleteProducto(id);
        return RedirectToAction(nameof(Index));
    }

    // Llenar los selectores para las vistas
    private void CargarCombos()
    {
        ViewBag.IdProveedor = new SelectList(provSer.GetProveedores(), "Id", "NombreEmpresa");
        // Nota: Las marcas suelen estar en DetalleProducto, 
        // pero cargamos el combo por si lo necesitas en el modelo principal
        ViewBag.IdMarca = new SelectList(marcSer.GetMarcas(), "Id", "Nombre");
    }
}