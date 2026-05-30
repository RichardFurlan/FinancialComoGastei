using Microsoft.AspNetCore.Identity;

namespace ComoGastoMinhaGrana.Domain.Entities;

public class User : IdentityUser<Guid>
{
    public string FullName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<FinancialStatement> FinancialStatements { get; set; } = new List<FinancialStatement>();
}
