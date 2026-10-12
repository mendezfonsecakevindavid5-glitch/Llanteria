using System.Security.Claims;
using Llanteria.Data;
using Llanteria.Infrastructure;
using Llanteria.Models;
using Llanteria.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Llanteria.Controllers;

public class ClienteController : Controller
{
    private readonly ClienteService ser;
    private readonly TipoDocumentoService docSer;
    private readonly SexoService sexoSer;
    private readonly LlanteriaDbContext _db;

    public ClienteController(ClienteService clienteService, TipoDocumentoService documentoService,
        SexoService sexoService, LlanteriaDbContext db)
    {
        ser = clienteService;
        docSer = documentoService;
        sexoSer = sexoService;
        _db = db;
    }

    public IActionResult Index(string? buscar, int pagina = 1)
    {
        ViewBag.Buscar = buscar;
        return View(Paginado<Cliente>.De(Filtrar(buscar), pagina));
    }

    public IActionResult Exportar(string? buscar)
    {
        var filas = Filtrar(buscar).ToList().Select(c => new string?[]
        {
            $"{c.Nombres} {c.Apellidos}".Trim(), c.IdDocumentoNavigation?.SiglasPresentacion, c.NumeroDocumento,
            c.Telefono, c.Correo, c.PuntosAcumulados.ToString()
        });
        return Csv.Archivo("clientes", new[] { "Cliente", "Tipo doc.", "Documento", "Teléfono", "Correo", "Puntos" }, filas);
    }

    private IQueryable<Cliente> Filtrar(string? buscar)
    {
        var q = _db.Clientes.AsNoTracking().Include(c => c.IdDocumentoNavigation).AsQueryable();
        if (!string.IsNullOrWhiteSpace(buscar))
        {
            var t = buscar.Trim();
            q = q.Where(c => c.Nombres.Contains(t) || c.Apellidos.Contains(t) || c.NumeroDocumento.Contains(t)
                             || c.Telefono.Contains(t) || c.Correo.Contains(t));
        }
        return q.OrderBy(c => c.Nombres).ThenBy(c => c.Apellidos);
    }

    public IActionResult Create()
    {
        CargarCombos();
        return View(new Cliente());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Create(Cliente c)
    {
        ValidarDuplicados(c);
        if (!ModelState.IsValid)
        {
            CargarCombos(c);
            return View(c);
        }

        c.PuntosAcumulados = 0;
        ser.AddCliente(c);
        TempData["Exito"] = "Cliente registrado.";
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Edit(int id)
    {
        var c = ser.GetCliente(id);
        if (c == null) return NotFound();
        CargarCombos(c);
        return View(c);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Edit(int id, Cliente c)
    {
        c.Id = id;
        var original = _db.Clientes.Find(id);
        if (original == null) return NotFound();

        ValidarDuplicados(c);
        if (!ModelState.IsValid)
        {
            c.PuntosAcumulados = original.PuntosAcumulados;
            CargarCombos(c);
            return View(c);
        }

        // Los puntos y la foto no se editan desde este formulario.
        original.Nombres = c.Nombres.Trim();
        original.Apellidos = c.Apellidos.Trim();
        original.IdDocumento = c.IdDocumento;
        original.NumeroDocumento = c.NumeroDocumento.Trim();
        original.Telefono = c.Telefono.Trim();
        original.Correo = c.Correo.Trim();
        original.IdSexo = c.IdSexo;
        _db.SaveChanges();
        TempData["Exito"] = "Cliente actualizado.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Delete(int id)
    {
        ser.DeleteCliente(id); // si tiene facturas o vehículos, el filtro global avisa del error
        TempData["Exito"] = "Cliente eliminado.";
        return RedirectToAction(nameof(Index));
    }

    // --- Puntos de fidelidad ---

    public IActionResult GestionPuntos()
    {
        ViewBag.Incentivos = _db.CatalogoIncentivos.AsNoTracking()
            .Where(p => p.StockDisponible == null || p.StockDisponible > 0)
            .OrderBy(p => p.PuntosRequeridos)
            .Select(p => new { id = p.Id, nombre = p.NombrePremio, puntos = p.PuntosRequeridos })
            .ToList();
        return View(ser.GetClientesPorPuntos());
    }

    // El canje lo registra el personal en el mostrador: descuenta puntos y stock del premio y deja constancia de quién lo entregó.
    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Canjear(int id, int idIncentivo)
    {
        var cliente = _db.Clientes.Find(id);
        var premio = _db.CatalogoIncentivos.Find(idIncentivo);
        if (cliente == null || premio == null) return NotFound();

        if (!int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var idUsuario))
        {
            TempData["Error"] = "No se pudo identificar al usuario que entrega el premio.";
            return RedirectToAction(nameof(GestionPuntos));
        }
        if (premio.StockDisponible is <= 0)
        {
            TempData["Error"] = $"«{premio.NombrePremio}» no tiene unidades disponibles.";
            return RedirectToAction(nameof(GestionPuntos));
        }
        if (cliente.PuntosAcumulados < premio.PuntosRequeridos)
        {
            TempData["Error"] = $"{cliente.Nombres} tiene {cliente.PuntosAcumulados:N0} puntos y el premio cuesta {premio.PuntosRequeridos:N0}.";
            return RedirectToAction(nameof(GestionPuntos));
        }

        cliente.PuntosAcumulados -= premio.PuntosRequeridos;
        if (premio.StockDisponible.HasValue) premio.StockDisponible--;
        _db.CanjeIncentivos.Add(new CanjeIncentivo
        {
            IdCliente = cliente.Id,
            IdIncentivo = premio.Id,
            FechaCanje = DateTime.Now,
            EntregadoPor = idUsuario
        });
        _db.SaveChanges();

        TempData["Exito"] = $"Canje registrado: {cliente.Nombres} {cliente.Apellidos} recibió «{premio.NombrePremio}» y le quedan {cliente.PuntosAcumulados:N0} puntos.";
        return RedirectToAction(nameof(GestionPuntos));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult ModificarPuntos(int id, int cantidad, string operacion)
    {
        var cliente = _db.Clientes.Find(id);
        if (cliente == null) return NotFound();

        if (cantidad <= 0 || (operacion != "sumar" && operacion != "restar"))
        {
            TempData["Error"] = "Indica una cantidad de puntos mayor a cero.";
            return RedirectToAction(nameof(GestionPuntos));
        }

        cliente.PuntosAcumulados = operacion == "sumar"
            ? cliente.PuntosAcumulados + cantidad
            : Math.Max(0, cliente.PuntosAcumulados - cantidad);
        _db.SaveChanges();

        TempData["Exito"] = $"{cliente.Nombres} {cliente.Apellidos} ahora tiene {cliente.PuntosAcumulados:N0} puntos.";
        return RedirectToAction(nameof(GestionPuntos));
    }

    private void ValidarDuplicados(Cliente c)
    {
        var doc = (c.NumeroDocumento ?? "").Trim();
        var correo = (c.Correo ?? "").Trim().ToLower();
        if (_db.Clientes.AsNoTracking().Any(x => x.Id != c.Id && x.NumeroDocumento == doc))
            ModelState.AddModelError(nameof(Cliente.NumeroDocumento), "Ya existe un cliente con ese documento.");
        if (_db.Clientes.AsNoTracking().Any(x => x.Id != c.Id && x.Correo.ToLower() == correo))
            ModelState.AddModelError(nameof(Cliente.Correo), "Ya existe un cliente con ese correo.");
    }

    private void CargarCombos(Cliente? c = null)
    {
        ViewBag.IdDocumento = new SelectList(docSer.GetTipoDocumentos(), "Id", "Nombre", c?.IdDocumento);
        ViewBag.IdSexo = new SelectList(sexoSer.GetSexos(), "Id", "Nombre", c?.IdSexo);
    }
}
