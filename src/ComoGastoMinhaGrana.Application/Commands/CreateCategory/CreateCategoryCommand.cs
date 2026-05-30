using ComoGastoMinhaGrana.Application.Common.Dtos;
using MediatR;

namespace ComoGastoMinhaGrana.Application.Commands.CreateCategory;

public record CreateCategoryCommand(Guid UserId, string Name, string Color)
    : IRequest<CreateCategoryResult>;

public record CreateCategoryResult(CategoryDto? Category, CreateCategoryError Error = CreateCategoryError.None);

public enum CreateCategoryError { None, DuplicateName }
