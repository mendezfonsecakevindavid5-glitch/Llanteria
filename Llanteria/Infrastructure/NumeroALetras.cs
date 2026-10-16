namespace Llanteria.Infrastructure;

/// <summary>Convierte un valor en pesos a letras para el pie de la factura ("SON: ... PESOS M/CTE").</summary>
public static class NumeroALetras
{
    private static readonly string[] Unidades =
    {
        "CERO", "UNO", "DOS", "TRES", "CUATRO", "CINCO", "SEIS", "SIETE", "OCHO", "NUEVE", "DIEZ", "ONCE", "DOCE", "TRECE",
        "CATORCE", "QUINCE", "DIECISÉIS", "DIECISIETE", "DIECIOCHO", "DIECINUEVE", "VEINTE", "VEINTIUNO", "VEINTIDÓS",
        "VEINTITRÉS", "VEINTICUATRO", "VEINTICINCO", "VEINTISÉIS", "VEINTISIETE", "VEINTIOCHO", "VEINTINUEVE"
    };
    private static readonly string[] Decenas =
        { "", "", "", "TREINTA", "CUARENTA", "CINCUENTA", "SESENTA", "SETENTA", "OCHENTA", "NOVENTA" };
    private static readonly string[] Centenas =
        { "", "CIENTO", "DOSCIENTOS", "TRESCIENTOS", "CUATROCIENTOS", "QUINIENTOS", "SEISCIENTOS", "SETECIENTOS", "OCHOCIENTOS", "NOVECIENTOS" };

    // 1..999. Con "apocopar", UNO pasa a UN y VEINTIUNO a VEINTIÚN (UN MIL, VEINTIÚN PESOS...).
    private static string Menor1000(int n, bool apocopar)
    {
        if (n == 100) return "CIEN";
        var partes = new List<string>();
        var c = n / 100;
        var r = n % 100;
        if (c > 0) partes.Add(Centenas[c]);
        if (r > 0)
        {
            if (r < 30)
            {
                var t = Unidades[r];
                if (apocopar)
                {
                    if (r == 1) t = "UN";
                    else if (r == 21) t = "VEINTIÚN";
                }
                partes.Add(t);
            }
            else
            {
                var d = r / 10;
                var u = r % 10;
                if (u == 0) partes.Add(Decenas[d]);
                else partes.Add(Decenas[d] + " Y " + (apocopar && u == 1 ? "UN" : Unidades[u]));
            }
        }
        return string.Join(" ", partes);
    }

    // 1..999.999
    private static string Grupo(long n, bool apocopar)
    {
        var partes = new List<string>();
        var miles = n / 1000;
        var resto = (int)(n % 1000);
        if (miles > 0) partes.Add(miles == 1 ? "MIL" : Menor1000((int)miles, true) + " MIL");
        if (resto > 0) partes.Add(Menor1000(resto, apocopar));
        return string.Join(" ", partes);
    }

    public static string Pesos(decimal valor)
    {
        var n = (long)Math.Round(Math.Abs(valor), 0, MidpointRounding.AwayFromZero);
        if (n == 0) return "CERO PESOS M/CTE";

        var billones = n / 1_000_000_000_000L;
        var resto = n % 1_000_000_000_000L;
        var millones = resto / 1_000_000L;
        var unidades = resto % 1_000_000L;

        var partes = new List<string>();
        if (billones > 0) partes.Add(billones == 1 ? "UN BILLÓN" : Grupo(billones, true) + " BILLONES");
        if (millones > 0) partes.Add(millones == 1 ? "UN MILLÓN" : Grupo(millones, true) + " MILLONES");
        if (unidades > 0) partes.Add(Grupo(unidades, true));

        var sufijo = n == 1 ? " PESO" : unidades == 0 ? " DE PESOS" : " PESOS";
        return string.Join(" ", partes) + sufijo + " M/CTE";
    }
}
