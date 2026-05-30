using System.Security.Claims;
using ComoGastoMinhaGrana.Application.Commands.CreateCategoryRule;
using ComoGastoMinhaGrana.Application.Commands.DeleteCategoryRule;
using ComoGastoMinhaGrana.Application.Commands.UpdateCategoryRule;
using ComoGastoMinhaGrana.Application.Queries.GetCategoryRules;
using ComoGastoMinhaGrana.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ComoGastoMinhaGrana.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/category-rules")]
public class CategoryRulesController : ControllerBase
{
    private readonly IMediator _mediator;

    public CategoryRulesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetCategoryRulesQuery(GetUserId()), cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CategoryRuleRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new CreateCategoryRuleCommand(GetUserId(), request.SearchTerm, request.RuleMatchType, request.CategoryId),
            cancellationToken);
        return CreatedAtAction(nameof(List), result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] CategoryRuleRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new UpdateCategoryRuleCommand(id, GetUserId(), request.SearchTerm, request.RuleMatchType, request.CategoryId),
            cancellationToken);

        return result.Error switch
        {
            UpdateCategoryRuleError.NotFound => NotFound(),
            UpdateCategoryRuleError.Forbidden => Forbid(),
            _ => Ok(result.Rule)
        };
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var error = await _mediator.Send(new DeleteCategoryRuleCommand(id, GetUserId()), cancellationToken);

        return error switch
        {
            DeleteCategoryRuleError.NotFound => NotFound(),
            DeleteCategoryRuleError.Forbidden => Forbid(),
            _ => NoContent()
        };
    }

    private Guid GetUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(value, out var id) ? id : throw new UnauthorizedAccessException();
    }
}

public record CategoryRuleRequest(string SearchTerm, RuleMatchType RuleMatchType, Guid CategoryId);
