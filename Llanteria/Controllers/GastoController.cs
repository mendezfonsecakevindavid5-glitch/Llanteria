using Microsoft.AspNetCore.Mvc;
using Llanteria.Models;
using Llanteria.Services;
using Llanteria.Filters; // 👈 NECESARIO: Namespace de tu filtro

namespace Llanteria.Controllers
{
    public class GastoController : Controller
    {
        private readonly GastoService ser;

        public GastoController(GastoService gastoService)
        {
            ser = gastoService;
        }

        public IActionResult Index() => View(ser.GetGastos());

        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        // 👈 AUDITORÍA: Registrar creación de egreso
        [TypeFilter(typeof(LogActionFilter), Arguments = new object[] { "Registró un nuevo gasto", "Gasto" })]
        public IActionResult Create(Gasto g)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    ser.AddGasto(g);
                    return RedirectToAction(nameof(Index));
                }
                return View(g);
            }
            catch
            {
                return View(g);
            }
        }

        public IActionResult Edit(int id)
        {
            var g = ser.GetGasto(id);
            if (g == null) return NotFound();
            return View(g);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        // 👈 AUDITORÍA: Registrar edición de egreso
        [TypeFilter(typeof(LogActionFilter), Arguments = new object[] { "Editó un gasto", "Gasto" })]
        public IActionResult Edit(int id, Gasto g)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    ser.UpdateGasto(g);
                    return RedirectToAction(nameof(Index));
                }
                return View(g);
            }
            catch
            {
                return View(g);
            }
        }

        // 👈 AUDITORÍA: Registrar eliminación de egreso
        [TypeFilter(typeof(LogActionFilter), Arguments = new object[] { "Eliminó un gasto", "Gasto" })]
        public IActionResult Delete(int id)
        {
            ser.DeleteGasto(id);
            return RedirectToAction(nameof(Index));
        }
    }
}