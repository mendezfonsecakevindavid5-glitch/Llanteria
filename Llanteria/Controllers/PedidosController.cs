using Llanteria.Data;
using Llanteria.Infrastructure;
using Llanteria.Models;
using Llanteria.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Llanteria.Controllers;

/// <summary>Checkout y confirmación (públicos) y gestión de pedidos (personal).</summary>
public class PedidosController : Controller
{
    private const string RolesPersonal = "Administrador,Empleado";

    private readonly LlanteriaDbContext _db;
    private readonly EmailService _email;
    private readonly NegocioInfo _negocio;

    public PedidosController(LlanteriaDbContext db, EmailService email, NegocioInfo negocio)
    {
        _db = db;
        _email = email;
        _negocio = negocio;
    }

    private int? IdCliente => int.TryParse(User.FindFirst("ClienteId")?.Value, out var id) ? id : null;

    // ---------- Checkout ----------

    [HttpGet]
    public IActionResult Checkout()
    {
        var cart = Carrito.Leer(Request);
        var resumen = ResumenCompra.Armar(_db, cart, Carrito.Cupon(Request));
        if (resumen.Lineas.Count == 0)
        {
            TempData["Error"] = "Tu carrito está vacío.";
            return RedirectToAction("Index", "Carrito");
        }
        if (resumen.HayProblemas)
        {
            TempData["Error"] = "Ajusta las cantidades: algún producto superó el stock disponible.";
            return RedirectToAction("Index", "Carrito");
        }

        var vm = new CheckoutViewModel();
        if (IdCliente is int idc)
        {
            var c = _db.Clientes.AsNoTracking().FirstOrDefault(x => x.Id == idc);
            if (c != null)
            {
                vm.Nombres = c.Nombres;
                vm.Apellidos = c.Apellidos;
                vm.NumeroDocumento = c.NumeroDocumento;
                vm.Correo = c.Correo;
                vm.Telefono = c.Telefono;
                var siglas = _db.TipoDocumentos.AsNoTracking().Where(t => t.Id == c.IdDocumento).Select(t => t.SiglasPresentacion).FirstOrDefault();
                if (siglas != null && CheckoutViewModel.TiposDocumento.Contains(siglas)) vm.TipoDocumento = siglas;
            }
        }
        ViewBag.Resumen = resumen;
        return View(vm);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Checkout(CheckoutViewModel vm)
    {
        var cart = Carrito.Leer(Request);
        var resumen = ResumenCompra.Armar(_db, cart, Carrito.Cupon(Request));
        if (resumen.Lineas.Count == 0)
        {
            TempData["Error"] = "Tu carrito está vacío.";
            return RedirectToAction("Index", "Carrito");
        }

        Normalizar(vm);
        if (!vm.AceptaTerminos)
            ModelState.AddModelError(nameof(vm.AceptaTerminos), "Debes aceptar los términos y el tratamiento de tus datos.");
        if (!CheckoutViewModel.Paises.Contains(vm.Pais))
            ModelState.AddModelError(nameof(vm.Pais), "Selecciona un país de la lista.");
        if (vm.MetodoPago is not (Pedido.Efectivo or Pedido.Transferencia))
            ModelState.AddModelError(nameof(vm.MetodoPago), "Elige cómo vas a pagar.");
        else if (vm.MetodoPago == Pedido.Efectivo && vm.Pais != "Colombia")
            ModelState.AddModelError(nameof(vm.MetodoPago), "El pago en efectivo solo está disponible para entregas en Colombia.");

        // El stock se descuenta en el contexto; si algo falla, no se guarda nada.
        foreach (var l in resumen.Lineas)
        {
            var error = StockHelper.Descontar(_db, l.IdProducto, l.Cantidad, l.Nombre);
            if (error != null) ModelState.AddModelError("", error);
        }

        if (!ModelState.IsValid)
        {
            ViewBag.Resumen = resumen;
            return View(vm);
        }

        var pedido = new Pedido
        {
            IdCliente = IdCliente,
            Nombres = vm.Nombres, Apellidos = vm.Apellidos, TipoDocumento = vm.TipoDocumento, NumeroDocumento = vm.NumeroDocumento,
            Correo = vm.Correo, Telefono = vm.Telefono,
            Pais = vm.Pais, Departamento = vm.Departamento, Ciudad = vm.Ciudad, Direccion = vm.Direccion,
            Complemento = vm.Complemento, CodigoPostal = vm.CodigoPostal, Notas = vm.Notas,
            MetodoPago = vm.MetodoPago, CodigoCupon = resumen.Cupon,
            Subtotal = resumen.Subtotal, Descuento = resumen.Descuento, BaseGravable = resumen.BaseGravable,
            Iva = resumen.Iva, Envio = resumen.Envio, Total = resumen.Total,
            Fecha = DateTime.Now
        };
        foreach (var l in resumen.Lineas)
            pedido.Items.Add(new PedidoItem { IdProducto = l.IdProducto, Descripcion = l.Nombre, Cantidad = l.Cantidad, ValorUnitario = l.Precio, Subtotal = l.Subtotal });

        using (var tx = _db.Database.BeginTransaction())
        {
            _db.Pedidos.Add(pedido);
            _db.SaveChanges(); // genera el Id
            pedido.Numero = $"LV-{DateTime.Now:yy}-{pedido.Id:D6}";
            _db.SaveChanges();
            tx.Commit();
        }

        Carrito.Vaciar(Response);
        await NotificarNuevoPedidoAsync(pedido);
        return RedirectToAction(nameof(Confirmacion), new { token = pedido.Token });
    }

    [HttpGet]
    public IActionResult Confirmacion(string token)
    {
        var p = _db.Pedidos.AsNoTracking().Include(x => x.Items).FirstOrDefault(x => x.Token == token);
        return p == null ? NotFound() : View(p);
    }

    private static void Normalizar(CheckoutViewModel v)
    {
        v.Nombres = (v.Nombres ?? "").Trim();
        v.Apellidos = (v.Apellidos ?? "").Trim();
        v.NumeroDocumento = (v.NumeroDocumento ?? "").Trim();
        v.Correo = (v.Correo ?? "").Trim();
        v.Telefono = (v.Telefono ?? "").Trim();
        v.Departamento = (v.Departamento ?? "").Trim();
        v.Ciudad = (v.Ciudad ?? "").Trim();
        v.Direccion = (v.Direccion ?? "").Trim();
        v.CodigoPostal = (v.CodigoPostal ?? "").Trim();
        v.Complemento = string.IsNullOrWhiteSpace(v.Complemento) ? null : v.Complemento.Trim();
        v.Notas = string.IsNullOrWhiteSpace(v.Notas) ? null : v.Notas.Trim();
    }

    // Los correos son un extra: si fallan, el pedido ya quedó guardado.
    private async Task NotificarNuevoPedidoAsync(Pedido p)
    {
        try
        {
            var detalle = string.Join("\n", p.Items.Select(i => $"{i.Cantidad} × {i.Descripcion} — {i.Subtotal:C0}"));
            var interno = EmailService.PlantillaAviso($"Nuevo pedido {p.Numero}", new (string, string?)[]
            {
                ("Cliente", p.NombreCompleto), ("Documento", $"{p.TipoDocumento} {p.NumeroDocumento}"), ("Teléfono", p.Telefono), ("Correo", p.Correo),
                ("Entrega", $"{p.Direccion}, {p.Ciudad}, {p.Departamento}, {p.Pais} ({p.CodigoPostal})"),
                ("Pago", p.MetodoPago), ("Total", p.Total.ToString("C0"))
            }, detalle + (string.IsNullOrEmpty(p.Notas) ? "" : $"\n\nNotas: {p.Notas}"));
            await _email.EnviarCorreoAsync(_email.CorreoDestino, "Llandupar", $"[Pedido] {p.Numero} – {p.NombreCompleto}", interno, responderA: p.Correo);

            var cliente = EmailService.PlantillaAviso("Recibimos tu pedido", new (string, string?)[]
            {
                ("Pedido", p.Numero), ("Total", p.Total.ToString("C0")), ("Forma de pago", p.MetodoPago)
            }, "Gracias por tu compra. Un asesor te contactará para confirmar la entrega.\n\n" + detalle);
            await _email.EnviarCorreoAsync(p.Correo, p.NombreCompleto, $"Pedido {p.Numero} – {_negocio.Nombre}", cliente);
        }
        catch { /* se ignora a propósito */ }
    }

    // ---------- Gestión (personal) ----------

    [Authorize(Roles = RolesPersonal)]
    public IActionResult Index(string? estado, string? buscar, int pagina = 1)
    {
        ViewBag.Estado = estado;
        ViewBag.Buscar = buscar;
        ViewBag.Pendientes = _db.Pedidos.Count(x => x.Estado == Pedido.Pendiente);
        ViewBag.PorEnviar = _db.Pedidos.Count(x => x.Estado == Pedido.Pagado);
        return View(Paginado<Pedido>.De(Filtrar(estado, buscar).OrderByDescending(x => x.Fecha), pagina));
    }

    [Authorize(Roles = RolesPersonal)]
    public IActionResult Detalle(int id)
    {
        var p = _db.Pedidos.AsNoTracking().Include(x => x.Items).FirstOrDefault(x => x.Id == id);
        return p == null ? NotFound() : View(p);
    }

    [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = RolesPersonal)]
    public async Task<IActionResult> CambiarEstado(int id, string estado, string? retorno)
    {
        var p = _db.Pedidos.Include(x => x.Items).FirstOrDefault(x => x.Id == id);
        if (p == null) return NotFound();

        if (!Pedido.Estados.Contains(estado)) TempData["Error"] = "Estado no válido.";
        else if (p.Estado == Pedido.Cancelado) TempData["Error"] = "Un pedido cancelado no se puede reactivar.";
        else
        {
            if (estado == Pedido.Cancelado)
                foreach (var i in p.Items.Where(i => i.IdProducto.HasValue))
                    StockHelper.Reponer(_db, i.IdProducto!.Value, i.Cantidad);

            p.Estado = estado;
            _db.SaveChanges();
            TempData["Exito"] = $"Pedido {p.Numero} marcado como {estado.ToLower()}.";

            if (estado is Pedido.Pagado or Pedido.Enviado or Pedido.Cancelado)
            {
                try
                {
                    var cuerpo = EmailService.PlantillaAviso($"Tu pedido está {estado.ToLower()}", new (string, string?)[]
                    {
                        ("Pedido", p.Numero), ("Total", p.Total.ToString("C0"))
                    }, estado == Pedido.Cancelado ? $"Si tienes dudas, escríbenos al {_negocio.Telefono}." : "Gracias por comprar en nuestro taller.");
                    await _email.EnviarCorreoAsync(p.Correo, p.NombreCompleto, $"Pedido {p.Numero}: {estado.ToLower()}", cuerpo);
                }
                catch { /* se ignora a propósito */ }
            }
        }
        return !string.IsNullOrEmpty(retorno) && Url.IsLocalUrl(retorno) ? LocalRedirect(retorno) : RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = RolesPersonal)]
    public IActionResult Exportar(string? estado, string? buscar)
    {
        var filas = Filtrar(estado, buscar).OrderByDescending(x => x.Fecha).ToList().Select(x => new string?[]
        {
            x.Numero, x.Fecha.ToString("dd/MM/yyyy HH:mm"), x.NombreCompleto, x.Telefono, x.Correo,
            $"{x.Direccion}, {x.Ciudad}, {x.Pais}", x.MetodoPago, x.Estado, x.Total.ToString("0.##")
        });
        return Csv.Archivo("pedidos", new[] { "Pedido", "Fecha", "Cliente", "Teléfono", "Correo", "Entrega", "Pago", "Estado", "Total" }, filas);
    }

    private IQueryable<Pedido> Filtrar(string? estado, string? buscar)
    {
        var q = _db.Pedidos.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(estado) && Pedido.Estados.Contains(estado)) q = q.Where(x => x.Estado == estado);
        if (!string.IsNullOrWhiteSpace(buscar))
        {
            var t = buscar.Trim();
            q = q.Where(x => x.Numero.Contains(t) || x.Nombres.Contains(t) || x.Apellidos.Contains(t) || x.Correo.Contains(t) || x.NumeroDocumento.Contains(t));
        }
        return q;
    }
}
