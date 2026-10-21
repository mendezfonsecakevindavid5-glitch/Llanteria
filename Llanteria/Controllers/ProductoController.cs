using ClosedXML.Excel;
using Llanteria.Models;
using Llanteria.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Llanteria.Controllers;

public class ProductoController : Controller
{
    private const string ImagenPorDefecto = "default-producto.png";
    private const string CarpetaImagenes = "productoos";

    private readonly ProductoService ser;
    private readonly ProveedoreService provSer;
    private readonly MarcaService marcSer;
    private readonly BodegaService bodSer;
    private readonly AlmacenImagenes _imagenes;

    public ProductoController(ProductoService productoService, ProveedoreService proveedoreService,
        MarcaService marcaService, BodegaService bodegaService, AlmacenImagenes imagenes)
    {
        ser = productoService;
        provSer = proveedoreService;
        marcSer = marcaService;
        bodSer = bodegaService;
        _imagenes = imagenes;
    }

    public IActionResult Index() => View(ser.GetProductos());

    public IActionResult Create()
    {
        CargarCombos();
        return View(new Producto { DetalleProducto = new DetalleProducto() });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Create(Producto p)
    {
        ValidarImagen(p.ImagenArchivo);
        ValidarDetalle(p);
        if (!ModelState.IsValid)
        {
            CargarCombos();
            return View(p);
        }

        LimpiarDetalle(p);
        p.RutaImagen = p.ImagenArchivo != null ? GuardarImagen(p.ImagenArchivo) : ImagenPorDefecto;
        ser.AddProducto(p);
        TempData["Exito"] = "Producto registrado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Edit(int id)
    {
        var producto = ser.GetProducto(id);
        if (producto == null) return NotFound();
        producto.DetalleProducto ??= new DetalleProducto();
        CargarCombos();
        return View(producto);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Edit(int id, Producto p)
    {
        p.Id = id;
        ValidarImagen(p.ImagenArchivo);
        ValidarDetalle(p);
        var original = ser.GetProducto(id);
        if (original == null) return NotFound();

        if (!ModelState.IsValid)
        {
            p.RutaImagen = original.RutaImagen;
            CargarCombos();
            return View(p);
        }

        LimpiarDetalle(p);
        var imagenAnterior = original.RutaImagen;
        if (p.ImagenArchivo != null)
        {
            p.RutaImagen = GuardarImagen(p.ImagenArchivo);
            EliminarImagen(imagenAnterior);
        }
        else
        {
            p.RutaImagen = imagenAnterior;
        }

        ser.UpdateProducto(p);
        TempData["Exito"] = "Producto actualizado.";
        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Lee la primera fila de datos de un Excel (.xlsx) subido por el usuario y devuelve los valores
    /// encontrados para precargar el formulario de producto. No guarda nada en la base de datos:
    /// solo "traduce" la hoja a los campos del formulario para que el usuario los revise y los envíe.
    /// Encabezados esperados en la primera fila (no sensibles a mayúsculas/acentos):
    /// Nombre, Categoria, Descripcion, Proveedor, Bodega, PrecioCompra, PrecioVenta,
    /// Marca, TipoVehiculo, Ancho, Perfil, Diametro, GarantiaMeses, Viscosidad, TipoAceite.
    /// </summary>
    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult ImportarExcel(IFormFile archivo)
    {
        if (archivo == null || archivo.Length == 0)
            return BadRequest(new { ok = false, error = "Selecciona un archivo de Excel (.xlsx)." });

        var extension = Path.GetExtension(archivo.FileName).ToLowerInvariant();
        if (extension != ".xlsx")
            return BadRequest(new { ok = false, error = "El archivo debe ser .xlsx." });

        try
        {
            using var stream = archivo.OpenReadStream();
            using var libro = new XLWorkbook(stream);
            var hoja = libro.Worksheets.First();
            var filaEncabezados = hoja.Row(1);
            if (filaEncabezados.IsEmpty())
                return BadRequest(new { ok = false, error = "La hoja está vacía." });

            // Mapa encabezado normalizado -> número de columna.
            var columnas = new Dictionary<string, int>();
            foreach (var celda in filaEncabezados.CellsUsed())
            {
                var titulo = NormalizarTexto(celda.GetString());
                if (!string.IsNullOrEmpty(titulo) && !columnas.ContainsKey(titulo))
                    columnas[titulo] = celda.Address.ColumnNumber;
            }

            var filaDatos = hoja.Row(2);
            if (filaDatos.IsEmpty())
                return BadRequest(new { ok = false, error = "No se encontraron datos en la fila 2. Deja la primera fila para los títulos de columna." });

            string? Texto(string columna) =>
                columnas.TryGetValue(NormalizarTexto(columna), out var col) ? filaDatos.Cell(col).GetString().Trim() : null;

            var avisos = new List<string>();
            var proveedorTexto = Texto("Proveedor");
            var bodegaTexto = Texto("Bodega");
            var marcaTexto = Texto("Marca");

            var idProveedor = BuscarIdPorNombre(provSer.GetProveedores().Select(p => (p.Id, p.NombreEmpresa)), proveedorTexto, avisos, "proveedor");
            var idBodega = BuscarIdPorNombre(bodSer.GetBodegas().Select(b => (b.Id, b.NombreBodega)), bodegaTexto, avisos, "bodega");
            var idMarca = BuscarIdPorNombre(marcSer.GetMarcas().Select(m => (m.Id, m.Nombre)), marcaTexto, avisos, "marca");

            var datos = new
            {
                nombre = Texto("Nombre"),
                categoria = Texto("Categoria"),
                descripcion = Texto("Descripcion"),
                idProveedor,
                idBodega,
                precioCompra = ADecimal(Texto("PrecioCompra")),
                precioVenta = ADecimal(Texto("PrecioVenta")),
                idMarca,
                tipoVehiculo = Llanteria.Models.TiposVehiculo.Normalizar(Texto("TipoVehiculo")),
                ancho = Texto("Ancho"),
                perfil = Texto("Perfil"),
                diametro = Texto("Diametro"),
                garantiaMeses = AEntero(Texto("GarantiaMeses")),
                viscosidad = Texto("Viscosidad"),
                tipoAceite = Texto("TipoAceite"),
                amperaje = Texto("Amperaje"),
                polaridad = Texto("Polaridad"),
            };

            return Json(new { ok = true, datos, avisos });
        }
        catch (Exception)
        {
            return BadRequest(new { ok = false, error = "No se pudo leer el archivo. Verifica que sea un Excel (.xlsx) válido." });
        }
    }

    private static string NormalizarTexto(string? texto)
    {
        if (string.IsNullOrWhiteSpace(texto)) return "";
        var formaD = texto.Trim().ToLowerInvariant().Normalize(System.Text.NormalizationForm.FormD);
        var sinAcentos = new string(formaD.Where(c => System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c) != System.Globalization.UnicodeCategory.NonSpacingMark).ToArray());
        return sinAcentos.Replace(" ", "");
    }

    private static int? BuscarIdPorNombre(IEnumerable<(int Id, string Nombre)> opciones, string? texto, List<string> avisos, string etiqueta)
    {
        if (string.IsNullOrWhiteSpace(texto)) return null;
        var coincidencia = opciones.FirstOrDefault(o => string.Equals(o.Nombre?.Trim(), texto.Trim(), StringComparison.OrdinalIgnoreCase));
        if (coincidencia.Id != 0) return coincidencia.Id;

        avisos.Add($"No se encontró el {etiqueta} «{texto}»; selecciónalo manualmente.");
        return null;
    }

    private static decimal? ADecimal(string? texto) =>
        decimal.TryParse(texto, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var valor) ? valor : null;

    private static int? AEntero(string? texto) =>
        int.TryParse(texto, out var valor) ? valor : null;

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Delete(int id)
    {
        var p = ser.GetProducto(id);
        if (p == null) return RedirectToAction(nameof(Index));
        var imagen = p.RutaImagen;
        ser.DeleteProducto(id);
        EliminarImagen(imagen);
        TempData["Exito"] = "Producto eliminado.";
        return RedirectToAction(nameof(Index));
    }

    private void ValidarDetalle(Producto p)
    {
        var d = p.DetalleProducto;
        if (d == null || d.IdMarca > 0) return;
        var conDatos = !string.IsNullOrWhiteSpace(d.Ancho) || !string.IsNullOrWhiteSpace(d.Perfil) || !string.IsNullOrWhiteSpace(d.Diametro)
                       || !string.IsNullOrWhiteSpace(d.Viscosidad) || !string.IsNullOrWhiteSpace(d.TipoAceite) || d.GarantiaMeses.HasValue;
        if (conDatos) ModelState.AddModelError("DetalleProducto.IdMarca", "Selecciona la marca para guardar la ficha técnica.");
    }

    // Si no se eligió marca, no se guarda el detalle (evita violar la llave foránea).
    private static void LimpiarDetalle(Producto p)
    {
        if (p.DetalleProducto == null || p.DetalleProducto.IdMarca <= 0) p.DetalleProducto = null;
    }

    private void ValidarImagen(IFormFile? archivo)
    {
        var error = AlmacenImagenes.Validar(archivo);
        if (error != null) ModelState.AddModelError(nameof(Producto.ImagenArchivo), error);
    }

    private string GuardarImagen(IFormFile archivo) => _imagenes.Guardar(archivo, CarpetaImagenes);

    private void EliminarImagen(string? nombre) => _imagenes.Eliminar(nombre, CarpetaImagenes, ImagenPorDefecto);

    private void CargarCombos()
    {
        ViewBag.IdProveedor = new SelectList(provSer.GetProveedores(), "Id", "NombreEmpresa");
        ViewBag.IdMarca = new SelectList(marcSer.GetMarcas(), "Id", "Nombre");
        ViewBag.IdBodega = new SelectList(bodSer.GetBodegas(), "Id", "NombreBodega");
    }
}
