namespace ComoGastoMinhaGrana.Application.Common.Dtos;

public record StatementDetailDto(
    Guid Id,
    string FileName,
    string FileExtension,
    DateTime UploadDate,
    string Status,
    string BaseCurrency,
    string? ErrorMessage,
    IReadOnlyList<TransactionDto> Transactions,
    bool HasAnalysis);
