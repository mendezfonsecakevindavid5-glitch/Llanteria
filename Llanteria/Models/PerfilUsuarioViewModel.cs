using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Llanteria.Models
{
    public class PerfilUsuarioViewModel
    {
       
        public int Id { get; set; }

        public string? Bio { get; set; }

        public string? TemaPreferencia { get; set; }

        public bool NotificacionesActivas { get; set; }

        public IFormFile? NuevaFoto { get; set; }

        public string? FotoBase64 { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        public string Nombre { get; set; } = null!;

        public string Correo { get; set; } = null!;

        [Required(ErrorMessage = "El teléfono es obligatorio")]
        public string Telefono { get; set; } = null!;

        public string? Direccion { get; set; }

        public int Puntos { get; set; }
    }
}