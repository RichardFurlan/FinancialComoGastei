using System.Text.RegularExpressions;
using ComoGastoMinhaGrana.Application.Common.Interfaces;

namespace ComoGastoMinhaGrana.Infrastructure.Services;

public partial class SanitizerService : ISanitizerService
{
    // CPF: 000.000.000-00
    [GeneratedRegex(@"\d{3}\.\d{3}\.\d{3}-\d{2}", RegexOptions.Compiled)]
    private static partial Regex CpfPattern();

    // CNPJ: 00.000.000/0000-00
    [GeneratedRegex(@"\d{2}\.\d{3}\.\d{3}/\d{4}-\d{2}", RegexOptions.Compiled)]
    private static partial Regex CnpjPattern();

    // Número de cartão de crédito: 16 dígitos (com ou sem espaços/hífens)
    [GeneratedRegex(@"\b(?:\d[ -]?){13,19}\d\b", RegexOptions.Compiled)]
    private static partial Regex CardNumberPattern();

    // Número de conta bancária: sequências do tipo "Ag 0000 / C/C 00000-0"
    [GeneratedRegex(@"(?:ag(?:ência)?|conta|c/?c|poupança)[^\d]*\d[\d.\- /]+", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex BankAccountPattern();

    // E-mail
    [GeneratedRegex(@"[a-zA-Z0-9._%+\-]+@[a-zA-Z0-9.\-]+\.[a-zA-Z]{2,}", RegexOptions.Compiled)]
    private static partial Regex EmailPattern();

    public string Sanitize(string rawText)
    {
        if (string.IsNullOrWhiteSpace(rawText)) return rawText;

        var result = CpfPattern().Replace(rawText, "[CPF omitido]");
        result = CnpjPattern().Replace(result, "[CNPJ omitido]");
        result = CardNumberPattern().Replace(result, "[Cartão omitido]");
        result = BankAccountPattern().Replace(result, "[Conta omitida]");
        result = EmailPattern().Replace(result, "[Email omitido]");

        return result;
    }
}
