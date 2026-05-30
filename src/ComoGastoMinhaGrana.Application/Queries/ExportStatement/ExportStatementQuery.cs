using MediatR;

namespace ComoGastoMinhaGrana.Application.Queries.ExportStatement;

public record ExportStatementQuery(Guid StatementId, Guid UserId, ExportFormat Format)
    : IRequest<ExportResult?>;

public enum ExportFormat { Csv, Xlsx, Pdf }

public record ExportResult(byte[] Content, string ContentType, string FileName);
