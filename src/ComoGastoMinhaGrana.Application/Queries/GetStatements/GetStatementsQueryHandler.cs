using ComoGastoMinhaGrana.Application.Common.Dtos;
using ComoGastoMinhaGrana.Application.Common.Interfaces;
using MediatR;

namespace ComoGastoMinhaGrana.Application.Queries.GetStatements;

public class GetStatementsQueryHandler : IRequestHandler<GetStatementsQuery, List<StatementSummaryDto>>
{
    private readonly IFinancialStatementRepository _repository;

    public GetStatementsQueryHandler(IFinancialStatementRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<StatementSummaryDto>> Handle(GetStatementsQuery request, CancellationToken cancellationToken)
    {
        var summaries = await _repository.GetSummariesByUserIdAsync(request.UserId);
        return summaries.ToList();
    }
}
