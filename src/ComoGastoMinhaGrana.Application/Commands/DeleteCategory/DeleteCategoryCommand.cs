using MediatR;

namespace ComoGastoMinhaGrana.Application.Commands.DeleteCategory;

public record DeleteCategoryCommand(Guid Id, Guid UserId) : IRequest<DeleteCategoryError>;

public enum DeleteCategoryError { None, NotFound, Forbidden }
