using ComoGastoMinhaGrana.Domain.Entities;
using ComoGastoMinhaGrana.Domain.Enums;

namespace ComoGastoMinhaGrana.Application.Services;

public class CategoryRuleApplierService
{
    /// <summary>
    /// Aplica as regras às transações sem categoria. Primeira regra que corresponder ganha.
    /// Regras são aplicadas na ordem de criação (já vêm ordenadas por CreatedAt).
    /// </summary>
    public IList<Transaction> Apply(IList<Transaction> transactions, IList<CategoryRule> rules)
    {
        if (rules.Count == 0) return transactions;

        var changed = new List<Transaction>();
        foreach (var transaction in transactions)
        {
            if (transaction.CategoryId is not null) continue;

            foreach (var rule in rules)
            {
                if (Matches(transaction.OriginalDescription, rule))
                {
                    transaction.CategoryId = rule.CategoryId;
                    changed.Add(transaction);
                    break;
                }
            }
        }
        return changed;
    }

    private static bool Matches(string description, CategoryRule rule) =>
        rule.RuleMatchType switch
        {
            RuleMatchType.Contains => description.Contains(rule.SearchTerm, StringComparison.OrdinalIgnoreCase),
            RuleMatchType.Exact => description.Equals(rule.SearchTerm, StringComparison.OrdinalIgnoreCase),
            RuleMatchType.StartsWith => description.StartsWith(rule.SearchTerm, StringComparison.OrdinalIgnoreCase),
            _ => false
        };
}
