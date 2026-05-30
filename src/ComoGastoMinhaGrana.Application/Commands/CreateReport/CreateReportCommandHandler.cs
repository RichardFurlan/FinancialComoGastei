using ComoGastoMinhaGrana.Application.Common.Dtos;
using ComoGastoMinhaGrana.Application.Common.Interfaces;
using ComoGastoMinhaGrana.Domain.Entities;
using MediatR;

namespace ComoGastoMinhaGrana.Application.Commands.CreateReport;

public class CreateReportCommandHandler : IRequestHandler<CreateReportCommand, CreateReportResult>
{
    private const int MaxStatementsPerReport = 6;
    private readonly IReportRepository _repository;

    public CreateReportCommandHandler(IReportRepository repository)
    {
        _repository = repository;
    }

    public async Task<CreateReportResult> Handle(CreateReportCommand request, CancellationToken cancellationToken)
    {
        if (request.StatementIds.Count == 0)
            return new CreateReportResult(null, CreateReportError.NoStatements);

        if (request.StatementIds.Count > MaxStatementsPerReport)
            return new CreateReportResult(null, CreateReportError.TooManyStatements);

        var report = new Report
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            Name = request.Name.Trim(),
            CreatedAt = DateTime.UtcNow,
            Statements = request.StatementIds
                .Distinct()
                .Select(sid => new ReportStatement { StatementId = sid })
                .ToList()
        };

        await _repository.AddAsync(report);

        return new CreateReportResult(
            new ReportSummaryDto(report.Id, report.Name, report.CreatedAt, report.Statements.Count));
    }
}
