using System.ComponentModel.DataAnnotations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema; // 👈 Obligatorio
using Microsoft.AspNetCore.Http; // 👈 Obligatorio para IFormFile
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Llanteria.Models;

public partial class Producto
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio.")]

    [StringLength(150)]

    public string Nombre { get; set; } = null!;

    public string? Descripcion { get; set; }

    [Range(0, 999999999, ErrorMessage = "El precio no puede ser negativo.")]

    public decimal PrecioCompra { get; set; }

    [Range(0, 999999999, ErrorMessage = "El precio no puede ser negativo.")]

    public decimal PrecioVenta { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Selecciona un proveedor.")]

    public int IdProveedor { get; set; }

    // 💡 Usaremos esta columna de texto existente para guardar el nombre del archivo (ej: "michelin-r16.jpg")
    public string? Categoria { get; set; }
    public string? RutaImagen { get; set; }


    public virtual ICollection<DetalleFactura> DetalleFacturas { get; set; } = new List<DetalleFactura>();

    public virtual DetalleProducto? DetalleProducto { get; set; }

    public virtual Proveedore? IdProveedorNavigation { get; set; } = null!;

    public virtual ICollection<Inventario> Inventarios { get; set; } = new List<Inventario>();

    // ✅ NUEVO: Captura el archivo binario en el formulario sin guardarse directamente en la BD
    [NotMapped]
    public IFormFile? ImagenArchivo { get; set; }

    // ✅ NUEVO: Relación con Bodega
    [Range(1, int.MaxValue, ErrorMessage = "Selecciona una bodega.")]
    public int IdBodega { get; set; }

    [ForeignKey("IdBodega")]
    public virtual Bodega? BodegaNavigation { get; set; }
}