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
        return await _repository.GetSummariesByUserIdAsync(request.UserId);
    }
}
