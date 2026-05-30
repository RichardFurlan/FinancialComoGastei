using ComoGastoMinhaGrana.Application.Common.Dtos;
using MediatR;

namespace ComoGastoMinhaGrana.Application.Queries.GetCategories;

public record GetCategoriesQuery(Guid UserId) : IRequest<IEnumerable<CategoryDto>>;
