using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema; // 👈 Obligatorio
using Microsoft.AspNetCore.Http; // 👈 Obligatorio para IFormFile

namespace Llanteria.Models;

public partial class Proveedore
{
    public int Id { get; set; }

    public string NombreEmpresa { get; set; } = null!;

    public string? Contacto { get; set; }

    public string? Telefono { get; set; }

    public virtual ICollection<Producto> Productos { get; set; } = new List<Producto>();

    // ✅ NUEVO: Captura el archivo binario en el formulario sin tocar la BD
    [NotMapped]
    public IFormFile? ImagenArchivo { get; set; }
}