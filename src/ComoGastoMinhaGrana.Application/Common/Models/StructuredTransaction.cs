namespace ComoGastoMinhaGrana.Application.Common.Models;

public class StructuredTransaction
{
    public DateTime Date { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
}
