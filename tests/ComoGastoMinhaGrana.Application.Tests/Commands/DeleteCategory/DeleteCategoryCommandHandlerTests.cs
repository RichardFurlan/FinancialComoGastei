using ComoGastoMinhaGrana.Application.Commands.DeleteCategory;
using ComoGastoMinhaGrana.Application.Common.Interfaces;
using ComoGastoMinhaGrana.Application.Tests.Common.Mothers;
using ComoGastoMinhaGrana.Domain.Entities;
using FluentAssertions;
using NSubstitute;

namespace ComoGastoMinhaGrana.Application.Tests.Commands.DeleteCategory;

public class DeleteCategoryCommandHandlerTests
{
    private readonly ICategoryRepository _repository = Substitute.For<ICategoryRepository>();
    private readonly DeleteCategoryCommandHandler _handler;

    public DeleteCategoryCommandHandlerTests()
    {
        _handler = new DeleteCategoryCommandHandler(_repository);
    }

    [Fact]
    public async Task Handle_WhenOwner_DeletesAndReturnsNone()
    {
        var userId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var category = CategoryMother.OwnedBy(userId, categoryId);

        _repository.GetByIdAsync(categoryId).Returns(category);

        var result = await _handler.Handle(new DeleteCategoryCommand(categoryId, userId), CancellationToken.None);

        result.Should().Be(DeleteCategoryError.None);
        await _repository.Received(1).DeleteAsync(category);
    }

    [Fact]
    public async Task Handle_WhenNotFound_ReturnsNotFound()
    {
        _repository.GetByIdAsync(Arg.Any<Guid>()).Returns((Category?)null);

        var result = await _handler.Handle(
            new DeleteCategoryCommand(Guid.NewGuid(), Guid.NewGuid()),
            CancellationToken.None);

        result.Should().Be(DeleteCategoryError.NotFound);
        await _repository.DidNotReceive().DeleteAsync(Arg.Any<Category>());
    }

    [Fact]
    public async Task Handle_WhenCategoryBelongsToAnotherUser_ReturnsForbidden()
    {
        var categoryId = Guid.NewGuid();
        var category = CategoryMother.Default(id: categoryId); // userId diferente

        _repository.GetByIdAsync(categoryId).Returns(category);

        var result = await _handler.Handle(
            new DeleteCategoryCommand(categoryId, Guid.NewGuid()),
            CancellationToken.None);

        result.Should().Be(DeleteCategoryError.Forbidden);
        await _repository.DidNotReceive().DeleteAsync(Arg.Any<Category>());
    }
}
