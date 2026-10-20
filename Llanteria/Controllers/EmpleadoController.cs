using Llanteria.Infrastructure;
using Llanteria.Models;
using Llanteria.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;

namespace Llanteria.Controllers;

public class EmpleadoController : Controller
{
    private static readonly string[] Extensiones = { ".jpg", ".jpeg", ".png", ".webp", ".gif", ".bmp" };

    private readonly EmpleadoService ser;
    private readonly TipoDocumentoService docSer;
    private readonly SexoService sexoSer;
    private readonly RoleService rolSer;

    public EmpleadoController(EmpleadoService empleadoService, TipoDocumentoService documentoService,
        SexoService sexoService, RoleService roleService)
    {
        ser = empleadoService;
        docSer = documentoService;
        sexoSer = sexoService;
        rolSer = roleService;
    }

    public IActionResult Index() => View(ser.GetEmpleados());

    // Sirve la foto guardada en la base de datos (evita incrustar imágenes en la lista).
    [HttpGet]
    public IActionResult Foto(int id)
    {
        var e = ser.GetEmpleado(id);
        if (e?.Foto == null || e.Foto.Length == 0) return NotFound();
        return File(e.Foto, "image/jpeg");
    }

    /// <summary>Foto del documento de identidad, solo para administración (datos sensibles).</summary>
    [HttpGet]
    public IActionResult FotoDocumento(int id)
    {
        var e = ser.GetEmpleado(id);
        if (e?.FotoDocumento == null || e.FotoDocumento.Length == 0) return NotFound();
        return File(e.FotoDocumento, "image/jpeg");
    }

    /// <summary>Foto del rostro del empleado, solo para administración (datos sensibles).</summary>
    [HttpGet]
    public IActionResult FotoRostro(int id)
    {
        var e = ser.GetEmpleado(id);
        if (e?.FotoRostro == null || e.FotoRostro.Length == 0) return NotFound();
        return File(e.FotoRostro, "image/jpeg");
    }

    public IActionResult Create()
    {
        CargarCombos();
        return View(new Empleado { FechaNacimiento = DateOnly.FromDateTime(DateTime.Today.AddYears(-25)) });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Create(Empleado e, IFormFile? fotoArchivo, IFormFile? documentoArchivo, IFormFile? rostroArchivo)
    {
        ValidarFoto(fotoArchivo, nameof(fotoArchivo));
        ValidarFoto(documentoArchivo, nameof(documentoArchivo));
        ValidarFoto(rostroArchivo, nameof(rostroArchivo));
        if (!ModelState.IsValid)
        {
            CargarCombos(e);
            return View(e);
        }

        if (fotoArchivo != null) e.Foto = ProcesarFoto(fotoArchivo, cuadrada: true);
        if (documentoArchivo != null) e.FotoDocumento = ProcesarFoto(documentoArchivo, cuadrada: false);
        if (rostroArchivo != null) e.FotoRostro = ProcesarFoto(rostroArchivo, cuadrada: true);
        ser.AddEmpleado(e);
        TempData["Exito"] = "Empleado registrado.";
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Edit(int id)
    {
        var e = ser.GetEmpleado(id);
        if (e == null) return NotFound();
        CargarCombos(e);
        return View(e);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Edit(int id, Empleado e, IFormFile? fotoArchivo, IFormFile? documentoArchivo, IFormFile? rostroArchivo)
    {
        ValidarFoto(fotoArchivo, nameof(fotoArchivo));
        ValidarFoto(documentoArchivo, nameof(documentoArchivo));
        ValidarFoto(rostroArchivo, nameof(rostroArchivo));
        var original = ser.GetEmpleado(id);
        if (original == null) return NotFound();
        if (!ModelState.IsValid)
        {
            e.Id = id;
            e.Foto = original.Foto;
            e.FotoDocumento = original.FotoDocumento;
            e.FotoRostro = original.FotoRostro;
            CargarCombos(e);
            return View(e);
        }

        // Se copian los campos del formulario sobre la entidad cargada: así la foto no se pierde
        // ni se pisan datos que el formulario no envía.
        original.Nombres = e.Nombres;
        original.Apellidos = e.Apellidos;
        original.IdDocumento = e.IdDocumento;
        original.NumeroDocumento = e.NumeroDocumento;
        original.Correo = e.Correo;
        original.Telefono = e.Telefono;
        original.Salario = e.Salario;
        original.Horario = e.Horario;
        original.IdSexo = e.IdSexo;
        original.FechaNacimiento = e.FechaNacimiento;
        original.IdRol = e.IdRol;
        if (fotoArchivo != null) original.Foto = ProcesarFoto(fotoArchivo, cuadrada: true);
        if (documentoArchivo != null) original.FotoDocumento = ProcesarFoto(documentoArchivo, cuadrada: false);
        if (rostroArchivo != null) original.FotoRostro = ProcesarFoto(rostroArchivo, cuadrada: true);

        ser.UpdateEmpleado(original);
        TempData["Exito"] = "Empleado actualizado.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Delete(int id)
    {
        ser.DeleteEmpleado(id); // si tiene usuario asociado, el filtro global avisa del error
        TempData["Exito"] = "Empleado eliminado.";
        return RedirectToAction(nameof(Index));
    }

    private void ValidarFoto(IFormFile? archivo, string campo)
    {
        if (archivo == null) return;
        if (!Extensiones.Contains(Path.GetExtension(archivo.FileName).ToLowerInvariant()))
            ModelState.AddModelError(campo, "El archivo debe ser una imagen.");
        else if (archivo.Length > 5 * 1024 * 1024)
            ModelState.AddModelError(campo, "La foto no puede superar 5 MB.");
    }

    /// <summary>Redimensiona la foto antes de guardarla. "cuadrada" recorta a 400x400 (foto de perfil o rostro);
    /// si no, solo limita el ancho máximo sin recortar (foto del documento, para no perder información).</summary>
    private static byte[] ProcesarFoto(IFormFile foto, bool cuadrada)
    {
        using var stream = foto.OpenReadStream();
        using var imagen = Image.Load(stream);
        var opciones = cuadrada
            ? new ResizeOptions { Size = new Size(400, 400), Mode = ResizeMode.Crop }
            : new ResizeOptions { Size = new Size(1200, 0), Mode = ResizeMode.Max };
        imagen.Mutate(x => x.Resize(opciones));
        using var salida = new MemoryStream();
        imagen.Save(salida, new JpegEncoder { Quality = 80 });
        return salida.ToArray();
    }

    private void CargarCombos(Empleado? e = null)
    {
        ViewBag.IdDocumento = new SelectList(docSer.GetTipoDocumentos(), "Id", "Nombre", e?.IdDocumento);
        ViewBag.IdSexo = new SelectList(sexoSer.GetSexos(), "Id", "Nombre", e?.IdSexo);
        ViewBag.IdRol = new SelectList(rolSer.GetRoles().Where(r => r.NombreRol != "Cliente" && r.NombreRol != RolesApp.UsuarioRegistrado), "Id", "NombreRol", e?.IdRol);
    }
}
