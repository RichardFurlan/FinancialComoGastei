using ComoGastoMinhaGrana.Application.Common.Interfaces;
using MediatR;

namespace ComoGastoMinhaGrana.Application.Queries.GetStatementAnalysis;

public class GetStatementAnalysisQueryHandler : IRequestHandler<GetStatementAnalysisQuery, string?>
{
    private readonly IFinancialStatementRepository _repository;
    private readonly IAnalysisCacheService _cache;

    public GetStatementAnalysisQueryHandler(
        IFinancialStatementRepository repository,
        IAnalysisCacheService cache)
    {
        _repository = repository;
        _cache = cache;
    }

    public async Task<string?> Handle(GetStatementAnalysisQuery request, CancellationToken cancellationToken)
    {
        var cached = await _cache.GetAsync(request.StatementId);
        if (cached is not null) return cached;

        var statement = await _repository.GetByIdAndUserIdAsync(request.StatementId, request.UserId);
        if (statement?.Analysis is null) return null;

        await _cache.SetAsync(request.StatementId, statement.Analysis.MarkdownContent, TimeSpan.FromDays(7));
        return statement.Analysis.MarkdownContent;
    }
}
