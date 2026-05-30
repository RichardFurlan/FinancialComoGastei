using ComoGastoMinhaGrana.Application.Common.Dtos;
using ComoGastoMinhaGrana.Application.Common.Interfaces;
using MediatR;

namespace ComoGastoMinhaGrana.Application.Queries.GetReports;

public class GetReportsQueryHandler : IRequestHandler<GetReportsQuery, IEnumerable<ReportSummaryDto>>
{
    private readonly IReportRepository _repository;

    public GetReportsQueryHandler(IReportRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<ReportSummaryDto>> Handle(GetReportsQuery request, CancellationToken cancellationToken)
    {
        var reports = await _repository.GetByUserIdAsync(request.UserId);
        return reports.Select(r => new ReportSummaryDto(r.Id, r.Name, r.CreatedAt, r.Statements.Count));
    }
}
