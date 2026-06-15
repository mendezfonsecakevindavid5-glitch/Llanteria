using Microsoft.AspNetCore.Mvc;
using Llanteria.Models;
using Llanteria.Services;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Llanteria.Controllers;

public class EmpleadoController : Controller
{
    private readonly EmpleadoService ser;
    private readonly TipoDocumentoService docSer;
    private readonly SexoService sexoSer;
    private readonly RoleService rolSer; // Asegúrate de tener este servicio creado

    public EmpleadoController(
        EmpleadoService empleadoService,
        TipoDocumentoService documentoService,
        SexoService sexoService,
        RoleService roleService)
    {
        ser = empleadoService;
        docSer = documentoService;
        sexoSer = sexoService;
        rolSer = roleService;
    }

    // Listado de empleados
    public IActionResult Index()
    {
        return View(ser.GetEmpleados());
    }

    // Vista para registro
    public IActionResult Create()
    {
        CargarCombos();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(Empleado e)
    {
        try
        {
            if (ModelState.IsValid)
            {
                ser.AddEmpleado(e);
                return RedirectToAction(nameof(Index));
            }
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", "Error al guardar: " + ex.Message);
        }
        CargarCombos();
        return View(e);
    }

    // Vista para editar
    public IActionResult Edit(int id)
    {
        var e = ser.GetEmpleado(id);
        if (e == null) return NotFound();

        CargarCombos();
        return View(e);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(int id, Empleado e)
    {
        try
        {
            if (ModelState.IsValid)
            {
                ser.UpdateEmpleado(e);
                return RedirectToAction(nameof(Index));
            }
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", "Error al actualizar: " + ex.Message);
        }
        CargarCombos();
        return View(e);
    }

    // Acción para eliminar
    public IActionResult Delete(int id)
    {
        ser.DeleteEmpleado(id);
        return RedirectToAction(nameof(Index));
    }

    // Método para llenar todos los selectores de la ficha de empleado
    private void CargarCombos()
    {
        ViewBag.IdDocumento = new SelectList(docSer.GetTipoDocumentos(), "Id", "Nombre");
        ViewBag.IdSexo = new SelectList(sexoSer.GetSexos(), "Id", "Nombre");
        // Usamos NombreRol porque así está en tu DbContext
        ViewBag.IdRol = new SelectList(rolSer.GetRoles(), "Id", "NombreRol");
    }
}