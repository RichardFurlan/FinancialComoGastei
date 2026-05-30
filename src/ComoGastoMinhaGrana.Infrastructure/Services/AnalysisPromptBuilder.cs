using System.Text;
using ComoGastoMinhaGrana.Application.Common.Models;

namespace ComoGastoMinhaGrana.Infrastructure.Services;

public static class AnalysisPromptBuilder
{
    public static string Build(IEnumerable<StructuredTransaction> transactions)
    {
        var list = transactions.ToList();
        if (list.Count == 0) return "Nenhuma transação encontrada.";

        var debits = list.Where(t => t.Amount < 0).ToList();
        var credits = list.Where(t => t.Amount >= 0).ToList();

        var totalDebits = debits.Sum(t => Math.Abs(t.Amount));
        var totalCredits = credits.Sum(t => t.Amount);

        var top5 = debits
            .OrderBy(t => t.Amount)
            .Take(5)
            .Select(t => $"- {t.Description}: R$ {Math.Abs(t.Amount):N2}");

        var currency = list.First().Currency;

        var sb = new StringBuilder();
        sb.AppendLine($"Período analisado: {list.Min(t => t.Date):dd/MM/yyyy} a {list.Max(t => t.Date):dd/MM/yyyy}");
        sb.AppendLine($"Moeda: {currency}");
        sb.AppendLine($"Total de transações: {list.Count}");
        sb.AppendLine($"Total de saídas: R$ {totalDebits:N2}");
        sb.AppendLine($"Total de entradas: R$ {totalCredits:N2}");
        sb.AppendLine($"Saldo do período: R$ {(totalCredits - totalDebits):N2}");
        sb.AppendLine();
        sb.AppendLine("Maiores gastos:");
        foreach (var item in top5) sb.AppendLine(item);
        sb.AppendLine();
        sb.AppendLine("Todas as transações (data, descrição, valor):");

        foreach (var t in list.OrderBy(t => t.Date))
            sb.AppendLine($"{t.Date:dd/MM/yyyy} | {t.Description} | {(t.Amount >= 0 ? "+" : "")}{t.Amount:N2} {t.Currency}");

        return sb.ToString();
    }
}
