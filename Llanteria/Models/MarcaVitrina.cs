namespace Llanteria.Models;

/// <summary>Marca para la franja del inicio: su logo (si existe el archivo) y a qué catálogo lleva.</summary>
public class MarcaVitrina
{
    public string Nombre { get; set; } = null!;
    public string? Logo { get; set; }
    /// <summary>"Llantas" o "Baterias" (nombre del controlador).</summary>
    public string Catalogo { get; set; } = "Llantas";
}
