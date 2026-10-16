using System.ComponentModel.DataAnnotations;

namespace Llanteria.Models;

public class CheckoutViewModel
{
    public static readonly string[] Paises =
        { "Colombia", "Estados Unidos", "México", "España", "Ecuador", "Perú", "Panamá", "Venezuela", "Otro" };
    public static readonly string[] TiposDocumento = { "C.C.", "T.I.", "C.E.", "NIT", "PAS" };

    [Required(ErrorMessage = "Escribe tus nombres."), StringLength(60)]
    public string Nombres { get; set; } = "";

    [Required(ErrorMessage = "Escribe tus apellidos."), StringLength(60)]
    public string Apellidos { get; set; } = "";

    [Required(ErrorMessage = "Selecciona el tipo de documento.")]
    public string TipoDocumento { get; set; } = "C.C.";

    [Required(ErrorMessage = "Escribe tu número de documento."), StringLength(20)]
    public string NumeroDocumento { get; set; } = "";

    [Required(ErrorMessage = "Escribe tu correo."), EmailAddress(ErrorMessage = "El correo no es válido."), StringLength(100)]
    public string Correo { get; set; } = "";

    [Required(ErrorMessage = "Escribe tu teléfono."), StringLength(25, MinimumLength = 7, ErrorMessage = "El teléfono no es válido.")]
    public string Telefono { get; set; } = "";

    [Required(ErrorMessage = "Selecciona el país de entrega.")]
    public string Pais { get; set; } = "Colombia";

    [Required(ErrorMessage = "Escribe el departamento o estado."), StringLength(60)]
    public string Departamento { get; set; } = "";

    [Required(ErrorMessage = "Escribe la ciudad."), StringLength(60)]
    public string Ciudad { get; set; } = "";

    [Required(ErrorMessage = "Escribe la dirección de entrega."), StringLength(200)]
    public string Direccion { get; set; } = "";

    [StringLength(100)]
    public string? Complemento { get; set; }

    [Required(ErrorMessage = "Escribe el código postal."), StringLength(12)]
    public string CodigoPostal { get; set; } = "";

    [StringLength(400)]
    public string? Notas { get; set; }

    [Required(ErrorMessage = "Elige cómo vas a pagar.")]
    public string MetodoPago { get; set; } = Pedido.Efectivo;

    public bool AceptaTerminos { get; set; }
}
