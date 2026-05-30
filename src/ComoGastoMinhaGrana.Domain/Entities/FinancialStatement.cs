using ComoGastoMinhaGrana.Domain.Enums;

namespace ComoGastoMinhaGrana.Domain.Entities;

public class FinancialStatement
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public string FileName { get; set; } = string.Empty;
    public string FileExtension { get; set; } = string.Empty;
    public DateTime UploadDate { get; set; }
    public StatementStatus Status { get; set; }
    public string BaseCurrency { get; set; } = "BRL";
    public string? ErrorMessage { get; set; }

    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    public FinancialAnalysis? Analysis { get; set; }
}
