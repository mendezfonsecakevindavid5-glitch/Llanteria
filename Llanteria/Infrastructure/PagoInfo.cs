namespace Llanteria.Infrastructure;

/// <summary>
/// Datos para recibir transferencias. Se llenan en appsettings.json, sección "Pagos".
/// Si están vacíos, la confirmación del pedido le indica al cliente que un asesor le enviará los datos.
/// </summary>
public class PagoInfo
{
    public string Titular { get; set; } = "";
    public string Nit { get; set; } = "";
    public string Banco { get; set; } = "";
    public string TipoCuenta { get; set; } = "Ahorros";
    public string NumeroCuenta { get; set; } = "";
    public string Nequi { get; set; } = "";

    public bool HayCuenta => !string.IsNullOrWhiteSpace(NumeroCuenta) || !string.IsNullOrWhiteSpace(Nequi);
}
