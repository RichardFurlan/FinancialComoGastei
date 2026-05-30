using ComoGastoMinhaGrana.Domain.Enums;

namespace ComoGastoMinhaGrana.Domain.Entities;

public class CategoryRule
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string SearchTerm { get; set; } = string.Empty;
    public Enums.RuleMatchType RuleMatchType { get; set; } = Enums.RuleMatchType.Contains;
    public Guid CategoryId { get; set; }
    public Category Category { get; set; } = null!;
    public int Priority { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
