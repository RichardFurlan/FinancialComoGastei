using ComoGastoMinhaGrana.Application.Common.Interfaces;
using ComoGastoMinhaGrana.Application.Queries.GetCategory;
using ComoGastoMinhaGrana.Domain.Entities;
using FluentAssertions;
using NSubstitute;

namespace ComoGastoMinhaGrana.Application.Tests.Queries.GetCategory;

public class GetCategoryQueryHandlerTests
{
    private readonly ICategoryRepository _repository = Substitute.For<ICategoryRepository>();
    private readonly GetCategoryQueryHandler _handler;

    public GetCategoryQueryHandlerTests()
    {
        _handler = new GetCategoryQueryHandler(_repository);
    }

    [Fact]
    public async Task Handle_WhenOwner_ReturnsDto()
    {
        var userId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var category = new Category { Id = categoryId, UserId = userId, Name = "Lazer", Color = "#AABBCC" };

        _repository.GetByIdAsync(categoryId).Returns(category);

        var result = await _handler.Handle(new GetCategoryQuery(categoryId, userId), CancellationToken.None);

        result.Should().NotBeNull();
        result!.Id.Should().Be(categoryId);
        result.Name.Should().Be("Lazer");
    }

    [Fact]
    public async Task Handle_WhenCategoryNotFound_ReturnsNull()
    {
        _repository.GetByIdAsync(Arg.Any<Guid>()).Returns((Category?)null);

        var result = await _handler.Handle(
            new GetCategoryQuery(Guid.NewGuid(), Guid.NewGuid()),
            CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WhenCategoryBelongsToAnotherUser_ReturnsNull()
    {
        var categoryId = Guid.NewGuid();
        var category = new Category { Id = categoryId, UserId = Guid.NewGuid() };

        _repository.GetByIdAsync(categoryId).Returns(category);

        var result = await _handler.Handle(
            new GetCategoryQuery(categoryId, Guid.NewGuid()),
            CancellationToken.None);

        result.Should().BeNull();
    }
}
