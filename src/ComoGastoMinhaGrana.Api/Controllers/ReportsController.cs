using System.Security.Claims;
using ComoGastoMinhaGrana.Application.Commands.CreateReport;
using ComoGastoMinhaGrana.Application.Commands.DeleteReport;
using ComoGastoMinhaGrana.Application.Queries.GetReportDetail;
using ComoGastoMinhaGrana.Application.Queries.GetReports;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ComoGastoMinhaGrana.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/reports")]
public class ReportsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ReportsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetReportsQuery(GetUserId()), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetReportDetailQuery(id, GetUserId()), cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateReportRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new CreateReportCommand(GetUserId(), request.Name, request.StatementIds),
            cancellationToken);

        return result.Error switch
        {
            CreateReportError.TooManyStatements => BadRequest("Máximo de 6 extratos por relatório."),
            CreateReportError.NoStatements => BadRequest("Selecione pelo menos 1 extrato."),
            _ => CreatedAtAction(nameof(Get), new { id = result.Report!.Id }, result.Report)
        };
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var error = await _mediator.Send(new DeleteReportCommand(id, GetUserId()), cancellationToken);

        return error switch
        {
            DeleteReportError.NotFound => NotFound(),
            DeleteReportError.Forbidden => Forbid(),
            _ => NoContent()
        };
    }

    private Guid GetUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(value, out var id) ? id : throw new UnauthorizedAccessException();
    }
}

public record CreateReportRequest(string Name, IList<Guid> StatementIds);
