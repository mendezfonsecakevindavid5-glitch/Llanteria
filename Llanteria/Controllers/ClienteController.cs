using Microsoft.AspNetCore.Mvc;
using Llanteria.Models;
using Llanteria.Services;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Llanteria.Controllers;

public class ClienteController : Controller
{
    private readonly ClienteService ser;
    // Necesitamos estos servicios para llenar los Selects de la vista
    private readonly TipoDocumentoService docSer;
    private readonly SexoService sexoSer;

    public ClienteController(ClienteService clienteService, TipoDocumentoService documentoService, SexoService sexoService)
    {
        ser = clienteService;
        docSer = documentoService;
        sexoSer = sexoService;
    }

    // Listado de clientes
    public IActionResult Index()
    {
        return View(ser.GetClientes());
    }

    // ✅ NUEVO: Vista de Administración y Control de Puntos
    public IActionResult GestionPuntos()
    {
        var clientes = ser.GetClientesPorPuntos();
        return View(clientes);
    }

    // ✅ NUEVO: Acción POST mediante AJAX para alterar puntos sin recargar la página
    [HttpPost]
    public IActionResult ModificarPuntos(int clienteId, int cantidad, string operacion)
    {
        var cliente = ser.GetCliente(clienteId);
        if (cliente == null)
        {
            return Json(new { success = false, message = "Cliente no encontrado en el sistema." });
        }

        int nuevosPuntos = cliente.PuntosAcumulados;

        if (operacion == "sumar")
        {
            nuevosPuntos += cantidad;
        }
        else if (operacion == "restar")
        {
            // Evita que los puntos queden en números negativos utilizando Math.Max
            nuevosPuntos = Math.Max(0, nuevosPuntos - cantidad);
        }

        // Guardamos los cambios llamando al servicio
        ser.ActualizarPuntos(clienteId, nuevosPuntos);

        return Json(new { success = true, nuevosPuntos = nuevosPuntos });
    }

    // Vista para registrar un nuevo cliente
    public IActionResult Create()
    {
        CargarCombos();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(Cliente c)
    {
        try
        {
            if (ModelState.IsValid)
            {
                ser.AddCliente(c);
                return RedirectToAction(nameof(Index));
            }
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", "Error al guardar: " + ex.Message);
        }
        CargarCombos();
        return View(c);
    }

    // Vista para editar información del cliente
    public IActionResult Edit(int id)
    {
        var c = ser.GetCliente(id);
        if (c == null) return NotFound();

        CargarCombos();
        return View(c);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(int id, Cliente c)
    {
        try
        {
            if (ModelState.IsValid)
            {
                ser.UpdateCliente(c);
                return RedirectToAction(nameof(Index));
            }
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", "Error al actualizar: " + ex.Message);
        }
        CargarCombos();
        return View(c);
    }

    // Acción para eliminar
    public IActionResult Delete(int id)
    {
        ser.DeleteCliente(id);
        return RedirectToAction(nameof(Index));
    }

    // Método privado para no repetir código de carga de Selects
    private void CargarCombos()
    {
        ViewBag.IdDocumento = new SelectList(docSer.GetTipoDocumentos(), "Id", "Nombre");
        ViewBag.IdSexo = new SelectList(sexoSer.GetSexos(), "Id", "Nombre");
    }
}