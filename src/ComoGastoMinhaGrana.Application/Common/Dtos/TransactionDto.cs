namespace ComoGastoMinhaGrana.Application.Common.Dtos;

public record TransactionDto(
    Guid Id,
    DateTime Date,
    string Description,
    decimal Amount,
    string Currency,
    Guid? CategoryId,
    string? CategoryName,
    string? CategoryColor);
