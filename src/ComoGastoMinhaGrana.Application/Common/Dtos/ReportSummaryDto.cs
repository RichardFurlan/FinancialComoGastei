namespace ComoGastoMinhaGrana.Application.Common.Dtos;

public record ReportSummaryDto(Guid Id, string Name, DateTime CreatedAt, int StatementCount);
