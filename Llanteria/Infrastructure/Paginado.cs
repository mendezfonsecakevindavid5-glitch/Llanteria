using Microsoft.EntityFrameworkCore;

namespace Llanteria.Infrastructure;

/// <summary>Resultado de una página de un listado (la consulta se corta en la base de datos, no en memoria).</summary>
public class Paginado<T>
{
    public const int TamanoPorDefecto = 20;

    public List<T> Items { get; init; } = new();
    public int Pagina { get; init; }
    public int TotalPaginas { get; init; }
    public int Total { get; init; }

    public static Paginado<T> De(IQueryable<T> consulta, int pagina, int tamano = TamanoPorDefecto)
    {
        var total = consulta.Count();
        var paginas = Math.Max(1, (int)Math.Ceiling(total / (double)tamano));
        pagina = Math.Clamp(pagina, 1, paginas);
        var items = consulta.Skip((pagina - 1) * tamano).Take(tamano).ToList();
        return new Paginado<T> { Items = items, Pagina = pagina, TotalPaginas = paginas, Total = total };
    }
}
