using ComoGastoMinhaGrana.Application.Common.Dtos;
using MediatR;

namespace ComoGastoMinhaGrana.Application.Commands.AssignCategory;

/// <summary>
/// Atribui ou remove uma categoria de uma transação.
/// Passe CategoryId = null para remover a categoria existente.
/// </summary>
public record AssignCategoryCommand(
    Guid TransactionId,
    Guid UserId,
    Guid? CategoryId) : IRequest<AssignCategoryResult>;

public enum AssignCategoryResult { Ok, TransactionNotFound, Forbidden, CategoryNotFound }
