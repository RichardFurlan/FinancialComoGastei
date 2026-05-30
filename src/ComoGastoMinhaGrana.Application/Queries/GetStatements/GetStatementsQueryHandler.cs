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
        var statements = await _repository.GetByUserIdAsync(request.UserId);

        return statements
            .OrderByDescending(s => s.UploadDate)
            .Select(s => new StatementSummaryDto(
                s.Id,
                s.FileName,
                s.FileExtension,
                s.UploadDate,
                s.Status.ToString(),
                s.Transactions.Count,
                s.Analysis is not null))
            .ToList();
    }
}
