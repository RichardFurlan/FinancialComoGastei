using ComoGastoMinhaGrana.Application.Common.Dtos;
using MediatR;

namespace ComoGastoMinhaGrana.Application.Queries.GetReports;

public record GetReportsQuery(Guid UserId) : IRequest<IEnumerable<ReportSummaryDto>>;
