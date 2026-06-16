using System.Collections.Generic;

namespace Llanteria.Models
{
    public class ReporteGeneralViewModel
    {
        // Esta clase es un "contenedor" para pasar múltiples listas a la vista
        public IEnumerable<Llanteria.Models.Factura> Facturas { get; set; }
        public IEnumerable<Llanteria.Models.Gasto> Gastos { get; set; }
    }
}