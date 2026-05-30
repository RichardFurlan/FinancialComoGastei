namespace ComoGastoMinhaGrana.Domain.Entities;

public class Transaction
{
    public Guid Id { get; set; }
    public DateTime Date { get; set; }
    public string OriginalDescription { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    
    public Guid? CategoryId { get; set; }
    public Category? Category { get; set; }

    public Guid FinancialStatementId { get; set; }
    public FinancialStatement FinancialStatement { get; set; } = null!;
}
