using ComoGastoMinhaGrana.Application.Common.Dtos;
using MediatR;

namespace ComoGastoMinhaGrana.Application.Queries.GetCategory;

public record GetCategoryQuery(Guid Id, Guid UserId) : IRequest<CategoryDto?>;
