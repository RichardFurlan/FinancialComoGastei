using System.Text.Json;
using ComoGastoMinhaGrana.Application.Common.Dtos;
using ComoGastoMinhaGrana.Application.Common.Interfaces;
using MediatR;

namespace ComoGastoMinhaGrana.Application.Queries.GetReportDetail;

public class GetReportDetailQueryHandler : IRequestHandler<GetReportDetailQuery, ReportDetailDto?>
{
    private readonly IReportRepository _repository;
    private readonly IReportCacheService _cache;

    public GetReportDetailQueryHandler(IReportRepository repository, IReportCacheService cache)
    {
        _repository = repository;
        _cache = cache;
    }

    public async Task<ReportDetailDto?> Handle(GetReportDetailQuery request, CancellationToken cancellationToken)
    {
        // Cache-first: verifica se já computamos esse relatório recentemente
        var cached = await _cache.GetAsync(request.Id);
        if (cached is not null)
        {
            var cachedDto = JsonSerializer.Deserialize<ReportDetailDto>(cached);
            if (cachedDto is not null && cachedDto.Id == request.Id)
                return cachedDto;
        }

        var report = await _repository.GetByIdWithStatementsAsync(request.Id);
        if (report is null || report.UserId != request.UserId) return null;

        var allTransactions = report.Statements
            .Select(rs => rs.Statement)
            .Where(s => s is not null)
            .SelectMany(s => s.Transactions)
            .ToList();

        // --- Totais por moeda ---
        var currencies = allTransactions
            .GroupBy(t => t.Currency)
            .Select(g =>
            {
                var debits = g.Where(t => t.Amount < 0).Sum(t => Math.Abs(t.Amount));
                var credits = g.Where(t => t.Amount >= 0).Sum(t => t.Amount);
                return new CurrencySummaryDto(g.Key, debits, credits, credits - debits);
            })
            .OrderByDescending(c => c.Debits)
            .ToList();

        // --- Totais por categoria (agregado) ---
        var categoryTotals = allTransactions
            .Where(t => t.Amount < 0)
            .GroupBy(t => new { Name = t.Category?.Name ?? "Sem categoria", Color = t.Category?.Color ?? "#94A3B8" })
            .Select(g => new CategorySummaryDto(g.Key.Name, g.Key.Color, g.Sum(t => Math.Abs(t.Amount))))
            .OrderByDescending(c => c.Total)
            .ToList();

        // --- Top 5 categorias por import ---
        var byImport = report.Statements
            .Select(rs => rs.Statement)
            .Where(s => s is not null)
            .Select(s =>
            {
                var top = s.Transactions
                    .Where(t => t.Amount < 0)
                    .GroupBy(t => new { Name = t.Category?.Name ?? "Sem categoria", Color = t.Category?.Color ?? "#94A3B8" })
                    .Select(g => new CategorySummaryDto(g.Key.Name, g.Key.Color, g.Sum(t => Math.Abs(t.Amount))))
                    .OrderByDescending(c => c.Total)
                    .Take(5)
                    .ToList();
                return new ImportCategoryComparisonDto(s.Id, s.FileName, top);
            })
            .ToList();

        // --- Top 5 maiores despesas ---
        var topExpenses = allTransactions
            .Where(t => t.Amount < 0)
            .OrderBy(t => t.Amount)
            .Take(5)
            .Select(t => new TopExpenseDto(t.Date, t.OriginalDescription, t.Amount, t.Currency, t.Category?.Name))
            .ToList();

        // --- Statements summary ---
        var statements = report.Statements
            .Select(rs => rs.Statement)
            .Where(s => s is not null)
            .Select(s => new StatementSummaryDto(
                s.Id, s.FileName, s.FileExtension,
                s.UploadDate, s.Status.ToString(),
                s.Transactions.Count, s.Analysis is not null))
            .ToList();

        var result = new ReportDetailDto(
            report.Id, report.Name, report.CreatedAt,
            allTransactions.Count,
            currencies, categoryTotals, byImport, topExpenses, statements);

        await _cache.SetAsync(request.Id, JsonSerializer.Serialize(result));

        return result;
    }
}
