using Microsoft.AspNetCore.Mvc;

namespace Llanteria.Controllers
{
    public class ServiciosController : Controller
    {
        [HttpGet("Servicios/montaje-balanceo")]
        public IActionResult MontajeBalanceo()
        {
            return View("~/Views/Servicios/montaje-balanceo.cshtml");
        }

        [HttpGet("Servicios/alineacion-3d")]
        public IActionResult Alineacion3D()
        {
            return View("~/Views/Servicios/alineacion-3d.cshtml");
        }

        [HttpGet("Servicios/mantenimiento-express")]
        public IActionResult MantenimientoExpress()
        {
            return View("~/Views/Servicios/mantenimiento-express.cshtml");
        }

        [HttpGet("Servicios/baterias")]
        public IActionResult Baterias()
        {
            return View("~/Views/Servicios/baterias.cshtml");
        }

        [HttpGet("Servicios/suspension-amortiguadores")]
        public IActionResult SuspensionAmortiguadores()
        {
            return View("~/Views/Servicios/suspension-amortiguadores.cshtml");
        }

        [HttpGet("Servicios/serviteca-movil")]
        public IActionResult ServitecaMovil()
        {
            return View("~/Views/Servicios/serviteca-movil.cshtml");
        }
    }
}