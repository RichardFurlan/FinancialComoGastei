using ComoGastoMinhaGrana.Domain.Entities;
using ComoGastoMinhaGrana.Domain.Enums;

namespace ComoGastoMinhaGrana.Application.Tests.Common.Mothers;

public static class FinancialStatementMother
{
    public static FinancialStatement Default(Guid? userId = null, StatementStatus status = StatementStatus.Processed) => new()
    {
        Id = Guid.NewGuid(),
        UserId = userId ?? Guid.NewGuid(),
        FileName = "extrato.pdf",
        FileExtension = ".pdf",
        UploadDate = DateTime.UtcNow,
        Status = status,
        BaseCurrency = "BRL"
    };

    public static FinancialStatement OwnedBy(Guid userId, StatementStatus status = StatementStatus.Processed)
        => Default(userId, status);

    public static FinancialStatement Processing(Guid? userId = null)
        => Default(userId, StatementStatus.Processing);
}
