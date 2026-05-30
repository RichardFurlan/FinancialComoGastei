using System.Text;
using ComoGastoMinhaGrana.Application.Common.Interfaces;
using MediatR;

namespace ComoGastoMinhaGrana.Application.Queries.ExportStatement;

public class ExportStatementQueryHandler : IRequestHandler<ExportStatementQuery, ExportResult?>
{
    private readonly IFinancialStatementRepository _statementRepository;
    private readonly IExportService _exportService;

    public ExportStatementQueryHandler(
        IFinancialStatementRepository statementRepository,
        IExportService exportService)
    {
        _statementRepository = statementRepository;
        _exportService = exportService;
    }

    public async Task<ExportResult?> Handle(ExportStatementQuery request, CancellationToken cancellationToken)
    {
        var statement = await _statementRepository.GetByIdAndUserIdAsync(request.StatementId, request.UserId);
        if (statement is null) return null;

        var safeName = Path.GetFileNameWithoutExtension(statement.FileName);

        return request.Format switch
        {
            ExportFormat.Csv => new ExportResult(
                _exportService.ToCsv(statement),
                "text/csv; charset=utf-8",
                $"{safeName}.csv"),

            ExportFormat.Xlsx => new ExportResult(
                _exportService.ToXlsx(statement),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"{safeName}.xlsx"),

            ExportFormat.Pdf => new ExportResult(
                _exportService.ToPdf(statement),
                "application/pdf",
                $"{safeName}.pdf"),

            _ => null
        };
    }
}
