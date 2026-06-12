using System.ComponentModel.DataAnnotations.Schema; // 👈 Asegúrate de tener este using
using Microsoft.AspNetCore.Http;

namespace Llanteria.Models;

public partial class TipoServicio
{
    public int Id { get; set; }
    public string NombreServicio { get; set; } = null!;
    public decimal ValorServicio { get; set; }
    public string? IconoPath { get; set; }

    public virtual ICollection<DetalleFactura> DetalleFacturas { get; set; } = new List<DetalleFactura>();

    // ✅ NUEVO: Esta propiedad recibirá el archivo del formulario, pero NO se guarda en la BD
    [NotMapped]
    public IFormFile? ImagenArchivo { get; set; }
}