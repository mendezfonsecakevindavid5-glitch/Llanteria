namespace Llanteria.Models
{
    public class Pais
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = null!;
        public string Prefijo { get; set; } = null!;
        public string CodigoISO { get; set; } = null!;
    }
}