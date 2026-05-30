using ComoGastoMinhaGrana.Domain.Entities;

namespace ComoGastoMinhaGrana.Application.Tests.Common.Mothers;

public static class TransactionMother
{
    public static Transaction Default(Guid? userId = null, Guid? categoryId = null) => new()
    {
        Id = Guid.NewGuid(),
        FinancialStatementId = Guid.NewGuid(),
        FinancialStatement = new FinancialStatement { UserId = userId ?? Guid.NewGuid() },
        Date = DateTime.UtcNow,
        OriginalDescription = "Compra supermercado",
        Amount = -150.00m,
        Currency = "BRL",
        CategoryId = categoryId
    };

    public static Transaction OwnedBy(Guid userId, Guid? categoryId = null)
        => Default(userId, categoryId);
}
