using System.Security.Claims;
using ComoGastoMinhaGrana.Application.Commands.CreateCategory;
using ComoGastoMinhaGrana.Application.Commands.DeleteCategory;
using ComoGastoMinhaGrana.Application.Commands.UpdateCategory;
using ComoGastoMinhaGrana.Application.Queries.GetCategories;
using ComoGastoMinhaGrana.Application.Queries.GetCategory;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ComoGastoMinhaGrana.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/categories")]
public class CategoriesController : ControllerBase
{
    private readonly IMediator _mediator;

    public CategoriesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetCategoriesQuery(GetUserId()), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetCategoryQuery(id, GetUserId()), cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CategoryRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new CreateCategoryCommand(GetUserId(), request.Name, request.Color),
            cancellationToken);

        return result.Error switch
        {
            CreateCategoryError.DuplicateName => Conflict("Já existe uma categoria com esse nome."),
            _ => CreatedAtAction(nameof(Get), new { id = result.Category!.Id }, result.Category)
        };
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] CategoryRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new UpdateCategoryCommand(id, GetUserId(), request.Name, request.Color),
            cancellationToken);

        return result.Error switch
        {
            UpdateCategoryError.NotFound => NotFound(),
            UpdateCategoryError.Forbidden => Forbid(),
            UpdateCategoryError.DuplicateName => Conflict("Já existe uma categoria com esse nome."),
            _ => Ok(result.Category)
        };
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var error = await _mediator.Send(new DeleteCategoryCommand(id, GetUserId()), cancellationToken);

        return error switch
        {
            DeleteCategoryError.NotFound => NotFound(),
            DeleteCategoryError.Forbidden => Forbid(),
            _ => NoContent()
        };
    }

    private Guid GetUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(value, out var id) ? id : throw new UnauthorizedAccessException();
    }
}

public record CategoryRequest(string Name, string Color);
