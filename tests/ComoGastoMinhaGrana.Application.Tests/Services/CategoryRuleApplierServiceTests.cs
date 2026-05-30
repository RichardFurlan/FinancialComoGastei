using ComoGastoMinhaGrana.Application.Services;
using ComoGastoMinhaGrana.Application.Tests.Common.Mothers;
using ComoGastoMinhaGrana.Domain.Entities;
using ComoGastoMinhaGrana.Domain.Enums;
using FluentAssertions;

namespace ComoGastoMinhaGrana.Application.Tests.Services;

public class CategoryRuleApplierServiceTests
{
    private readonly CategoryRuleApplierService _service = new();

    private static Transaction Transaction(string description, Guid? categoryId = null) => new()
    {
        Id = Guid.NewGuid(),
        OriginalDescription = description,
        Amount = -100m,
        Currency = "BRL",
        Date = DateTime.UtcNow,
        FinancialStatementId = Guid.NewGuid(),
        FinancialStatement = new FinancialStatement { UserId = Guid.NewGuid() },
        CategoryId = categoryId
    };

    private static CategoryRule Rule(string term, RuleMatchType type, Guid categoryId) => new()
    {
        Id = Guid.NewGuid(),
        SearchTerm = term,
        RuleMatchType = type,
        CategoryId = categoryId,
        UserId = Guid.NewGuid()
    };

    [Fact]
    public void Apply_Contains_CaseInsensitive_Matches()
    {
        var categoryId = Guid.NewGuid();
        var t = Transaction("Pagamento UBER VIAGEM");
        var rules = new List<CategoryRule> { Rule("uber", RuleMatchType.Contains, categoryId) };

        _service.Apply([t], rules);

        t.CategoryId.Should().Be(categoryId);
    }

    [Fact]
    public void Apply_Exact_MatchesOnlyExact()
    {
        var categoryId = Guid.NewGuid();
        var tExact = Transaction("IFOOD");
        var tPartial = Transaction("IFOOD RESTAURANTE");
        var rules = new List<CategoryRule> { Rule("IFOOD", RuleMatchType.Exact, categoryId) };

        _service.Apply([tExact, tPartial], rules);

        tExact.CategoryId.Should().Be(categoryId);
        tPartial.CategoryId.Should().BeNull();
    }

    [Fact]
    public void Apply_StartsWith_MatchesPrefix()
    {
        var categoryId = Guid.NewGuid();
        var t = Transaction("AMAZON PRIME VIDEO");
        var rules = new List<CategoryRule> { Rule("AMAZON", RuleMatchType.StartsWith, categoryId) };

        _service.Apply([t], rules);

        t.CategoryId.Should().Be(categoryId);
    }

    [Fact]
    public void Apply_DoesNotOverrideAlreadyCategorized()
    {
        var existingCategoryId = Guid.NewGuid();
        var newCategoryId = Guid.NewGuid();
        var t = Transaction("UBER", categoryId: existingCategoryId);
        var rules = new List<CategoryRule> { Rule("UBER", RuleMatchType.Contains, newCategoryId) };

        _service.Apply([t], rules);

        t.CategoryId.Should().Be(existingCategoryId);
    }

    [Fact]
    public void Apply_FirstRuleWins()
    {
        var firstCategoryId = Guid.NewGuid();
        var secondCategoryId = Guid.NewGuid();
        var t = Transaction("UBER EATS");
        var rules = new List<CategoryRule>
        {
            Rule("UBER", RuleMatchType.Contains, firstCategoryId),
            Rule("EATS", RuleMatchType.Contains, secondCategoryId),
        };

        _service.Apply([t], rules);

        t.CategoryId.Should().Be(firstCategoryId);
    }

    [Fact]
    public void Apply_NoRules_LeavesTransactionsUnchanged()
    {
        var t = Transaction("COMPRA DIVERSA");

        _service.Apply([t], []);

        t.CategoryId.Should().BeNull();
    }

    [Fact]
    public void Apply_ReturnsOnlyChangedTransactions()
    {
        var categoryId = Guid.NewGuid();
        var matched = Transaction("IFOOD");
        var unmatched = Transaction("COMPRA GENERICA");
        var rules = new List<CategoryRule> { Rule("IFOOD", RuleMatchType.Contains, categoryId) };

        var changed = _service.Apply([matched, unmatched], rules);

        changed.Should().HaveCount(1);
        changed[0].Id.Should().Be(matched.Id);
    }
}
