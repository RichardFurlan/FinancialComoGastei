namespace ComoGastoMinhaGrana.Domain.Entities;

public class FinancialAnalysis
{
    public Guid Id { get; set; }
    public Guid FinancialStatementId { get; set; }
    public FinancialStatement FinancialStatement { get; set; } = null!;

    public string MarkdownContent { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}
