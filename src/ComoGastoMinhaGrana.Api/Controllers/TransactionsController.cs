using System.Security.Claims;
using ComoGastoMinhaGrana.Application.Commands.AssignCategory;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ComoGastoMinhaGrana.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/transactions")]
public class TransactionsController : ControllerBase
{
    private readonly IMediator _mediator;

    public TransactionsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPatch("{id:guid}/category")]
    public async Task<IActionResult> AssignCategory(
        Guid id,
        [FromBody] AssignCategoryRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new AssignCategoryCommand(id, GetUserId(), request.CategoryId),
            cancellationToken);

        return result switch
        {
            AssignCategoryResult.Ok => NoContent(),
            AssignCategoryResult.TransactionNotFound => NotFound("Transação não encontrada."),
            AssignCategoryResult.Forbidden => Forbid(),
            AssignCategoryResult.CategoryNotFound => BadRequest("Categoria não encontrada."),
            _ => StatusCode(500)
        };
    }

    private Guid GetUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(value, out var id) ? id : throw new UnauthorizedAccessException();
    }
}

/// <param name="CategoryId">null para remover a categoria da transação.</param>
public record AssignCategoryRequest(Guid? CategoryId);
