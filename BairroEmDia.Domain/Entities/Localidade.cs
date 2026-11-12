using BairroEmDia.Domain.Exceptions;

namespace BairroEmDia.Domain.Entities;

public static class Localidade
{
    private static readonly HashSet<string> Ufs = new(StringComparer.OrdinalIgnoreCase)
    {
        "AC", "AL", "AP", "AM", "BA", "CE", "DF", "ES", "GO",
        "MA", "MT", "MS", "MG", "PA", "PB", "PR", "PE", "PI",
        "RJ", "RN", "RS", "RO", "RR", "SC", "SP", "SE", "TO"
    };

    public static string? NormalizarFiltro(string? valor)
        => string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();

    public static void ValidarUf(string? uf)
    {
        if (string.IsNullOrWhiteSpace(uf) || !Ufs.Contains(uf.Trim()))
            throw new RegraDeNegocioException("Informe uma UF brasileira válida.");
    }
}
