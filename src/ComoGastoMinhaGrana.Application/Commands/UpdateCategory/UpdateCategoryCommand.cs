using ComoGastoMinhaGrana.Application.Common.Dtos;
using MediatR;

namespace ComoGastoMinhaGrana.Application.Commands.UpdateCategory;

public record UpdateCategoryCommand(Guid Id, Guid UserId, string Name, string Color)
    : IRequest<UpdateCategoryResult>;

public record UpdateCategoryResult(CategoryDto? Category, UpdateCategoryError Error = UpdateCategoryError.None);

public enum UpdateCategoryError { None, NotFound, Forbidden, DuplicateName }
