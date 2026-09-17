using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema; // 👈 Obligatorio
using Microsoft.AspNetCore.Http; // 👈 Obligatorio para IFormFile
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Llanteria.Models;

public partial class Producto
{
    public int Id { get; set; }

    public string Nombre { get; set; } = null!;

    public string? Descripcion { get; set; }

    public decimal PrecioCompra { get; set; }

    public decimal PrecioVenta { get; set; }

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
    public int IdBodega { get; set; }

    [ForeignKey("IdBodega")]
    public virtual Bodega? BodegaNavigation { get; set; }
}