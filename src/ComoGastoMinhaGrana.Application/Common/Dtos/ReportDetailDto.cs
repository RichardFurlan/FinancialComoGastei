namespace ComoGastoMinhaGrana.Application.Common.Dtos;

public record ReportDetailDto(
    Guid Id,
    string Name,
    DateTime CreatedAt,
    int TotalTransactions,
    IList<CurrencySummaryDto> Currencies,
    IList<CategorySummaryDto> CategoryTotals,
    IList<ImportCategoryComparisonDto> ByImport,
    IList<TopExpenseDto> TopExpenses,
    IList<StatementSummaryDto> Statements);

public record CurrencySummaryDto(string Currency, decimal Debits, decimal Credits, decimal Balance);

public record CategorySummaryDto(string Name, string Color, decimal Total);

public record ImportCategoryComparisonDto(
    Guid StatementId,
    string FileName,
    IList<CategorySummaryDto> TopCategories);

public record TopExpenseDto(
    DateTime Date,
    string Description,
    decimal Amount,
    string Currency,
    string? CategoryName);
