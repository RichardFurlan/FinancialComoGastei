namespace ComoGastoMinhaGrana.Application.Common.Dtos;

public record StatementSummaryDto(
    Guid Id,
    string FileName,
    string FileExtension,
    DateTime UploadDate,
    string Status,
    int TransactionCount,
    bool HasAnalysis);
