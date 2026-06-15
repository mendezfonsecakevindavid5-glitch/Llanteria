using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Llanteria.Models;

public partial class Usuario
{
    [Key]
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre de usuario es obligatorio.")]
    [StringLength(50, MinimumLength = 4, ErrorMessage = "El Username debe tener entre 4 y 50 caracteres.")]
    [RegularExpression(@"^[a-zA-Z0-9_]+$", ErrorMessage = "El Username solo puede contener letras, números y guiones bajos.")]
    [Display(Name = "Nombre de Usuario")]
    public string Username { get; set; } = null!;

    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    [DataType(DataType.Password)]
    [Display(Name = "Contraseña")]
    public string PasswordHash { get; set; } = null!;

    [Required(ErrorMessage = "Debe asociar un empleado.")]
    [Display(Name = "Empleado")]
    public int IdEmpleado { get; set; }

    [Required(ErrorMessage = "El rol es obligatorio.")]
    [Display(Name = "Rol de Usuario")]
    public int IdRol { get; set; }

    [StringLength(20)]
    [RegularExpression("^(Activo|Inactivo|Baneado)$", ErrorMessage = "Estado no válido.")]
    public string? Estado { get; set; } = "Activo";

    [StringLength(255, ErrorMessage = "El motivo no puede exceder los 255 caracteres.")]
    [Display(Name = "Motivo de Sanción")]
    public string? MotivoSancion { get; set; }

    [DataType(DataType.DateTime)]
    [Display(Name = "Fecha Fin de Baneo")]
    public DateTime? FechaFinBaneo { get; set; }

    [Display(Name = "Fecha de Creación")]
    public DateTime? FechaCreacion { get; set; }

    [Display(Name = "Última Conexión")]
    public DateTime? UltimaConexion { get; set; }

    // --- Relaciones y Navegación ---

    public virtual ICollection<CanjeIncentivo> CanjeIncentivos { get; set; } = new List<CanjeIncentivo>();

    [ForeignKey("IdEmpleado")]
    [Display(Name = "Datos del Empleado")]
    public virtual Empleado IdEmpleadoNavigation { get; set; } = null!;

    [ForeignKey("IdRol")]
    [Display(Name = "Rol Asignado")]
    public virtual Role IdRolNavigation { get; set; } = null!;

    public virtual ICollection<LogActividad> LogActividads { get; set; } = new List<LogActividad>();

    public virtual PerfilUsuario? PerfilUsuario { get; set; }

    public virtual ICollection<RecuperacionCuenta> RecuperacionCuenta { get; set; } = new List<RecuperacionCuenta>();
}