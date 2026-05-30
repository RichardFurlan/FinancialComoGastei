namespace ComoGastoMinhaGrana.Domain.Entities;

public class Report
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<ReportStatement> Statements { get; set; } = new List<ReportStatement>();
}

public class ReportStatement
{
    public Guid ReportId { get; set; }
    public Report Report { get; set; } = null!;

    public Guid StatementId { get; set; }
    public FinancialStatement Statement { get; set; } = null!;
}
