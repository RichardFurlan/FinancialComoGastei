using System.Security.Claims;
using ComoGastoMinhaGrana.Application.Commands.ApplyRulesToStatement;
using ComoGastoMinhaGrana.Application.Commands.DeleteStatement;
using ComoGastoMinhaGrana.Application.Commands.UploadStatement;
using ComoGastoMinhaGrana.Application.Queries.ExportStatement;
using ComoGastoMinhaGrana.Application.Queries.GetStatementAnalysis;
using ComoGastoMinhaGrana.Application.Queries.GetStatementDetail;
using ComoGastoMinhaGrana.Application.Queries.GetStatements;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ComoGastoMinhaGrana.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/statements")]
public class StatementsController : ControllerBase
{
    private readonly IMediator _mediator;

    public StatementsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("upload")]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<IActionResult> Upload(IFormFile file, CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
            return BadRequest("Arquivo inválido.");

        await using var stream = file.OpenReadStream();
        var id = await _mediator.Send(new UploadStatementCommand
        {
            UserId = GetUserId(),
            FileName = file.FileName,
            FileStream = stream
        }, cancellationToken);

        return Accepted(new { id });
    }

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetStatementsQuery(GetUserId()), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetStatementDetailQuery(id, GetUserId()), cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet("{id:guid}/analysis")]
    public async Task<IActionResult> GetAnalysis(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetStatementAnalysisQuery(id, GetUserId()), cancellationToken);
        return result is null ? NotFound("Análise ainda não disponível.") : Ok(new { markdown = result });
    }

    [HttpGet("{id:guid}/export")]
    public async Task<IActionResult> Export(Guid id, [FromQuery] string format, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<ExportFormat>(format, ignoreCase: true, out var exportFormat))
            return BadRequest("Formato inválido. Use: csv, xlsx ou pdf.");

        var result = await _mediator.Send(new ExportStatementQuery(id, GetUserId(), exportFormat), cancellationToken);
        if (result is null) return NotFound();

        return File(result.Content, result.ContentType, result.FileName);
    }

    [HttpPost("{id:guid}/apply-rules")]
    public async Task<IActionResult> ApplyRules(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new ApplyRulesToStatementCommand(id, GetUserId()), cancellationToken);

        return result.Error switch
        {
            ApplyRulesError.NotFound => NotFound(),
            ApplyRulesError.Forbidden => Forbid(),
            _ => Ok(new { categorized = result.CategorizedCount })
        };
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var error = await _mediator.Send(new DeleteStatementCommand(id, GetUserId()), cancellationToken);

        return error switch
        {
            DeleteStatementError.NotFound => NotFound(),
            DeleteStatementError.Forbidden => Forbid(),
            DeleteStatementError.Processing => Conflict("Extrato está sendo processado. Aguarde a conclusão antes de excluir."),
            _ => NoContent()
        };
    }

    private Guid GetUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(value, out var id) ? id : throw new UnauthorizedAccessException();
    }
}
