using ComoGastoMinhaGrana.Application.Commands.AssignCategory;
using ComoGastoMinhaGrana.Application.Common.Interfaces;
using ComoGastoMinhaGrana.Application.Tests.Common.Mothers;
using ComoGastoMinhaGrana.Domain.Entities;
using FluentAssertions;
using NSubstitute;

namespace ComoGastoMinhaGrana.Application.Tests.Commands.AssignCategory;

public class AssignCategoryCommandHandlerTests
{
    private readonly ITransactionRepository _transactionRepository = Substitute.For<ITransactionRepository>();
    private readonly ICategoryRepository _categoryRepository = Substitute.For<ICategoryRepository>();
    private readonly AssignCategoryCommandHandler _handler;

    public AssignCategoryCommandHandlerTests()
    {
        _handler = new AssignCategoryCommandHandler(_transactionRepository, _categoryRepository);
    }

    [Fact]
    public async Task Handle_WhenOwnerAndCategoryBelongsToUser_AssignsCategory()
    {
        var userId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var transaction = TransactionMother.OwnedBy(userId);
        var category = CategoryMother.OwnedBy(userId, categoryId);

        _transactionRepository.GetByIdWithStatementAsync(transaction.Id).Returns(transaction);
        _categoryRepository.GetByIdAsync(categoryId).Returns(category);

        var result = await _handler.Handle(
            new AssignCategoryCommand(transaction.Id, userId, categoryId),
            CancellationToken.None);

        result.Should().Be(AssignCategoryResult.Ok);
        transaction.CategoryId.Should().Be(categoryId);
        await _transactionRepository.Received(1).UpdateAsync(transaction);
    }

    [Fact]
    public async Task Handle_WithNullCategoryId_RemovesCategory()
    {
        var userId = Guid.NewGuid();
        var transaction = TransactionMother.OwnedBy(userId, categoryId: Guid.NewGuid());

        _transactionRepository.GetByIdWithStatementAsync(transaction.Id).Returns(transaction);

        var result = await _handler.Handle(
            new AssignCategoryCommand(transaction.Id, userId, null),
            CancellationToken.None);

        result.Should().Be(AssignCategoryResult.Ok);
        transaction.CategoryId.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WhenTransactionNotFound_ReturnsTransactionNotFound()
    {
        _transactionRepository.GetByIdWithStatementAsync(Arg.Any<Guid>()).Returns((Transaction?)null);

        var result = await _handler.Handle(
            new AssignCategoryCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()),
            CancellationToken.None);

        result.Should().Be(AssignCategoryResult.TransactionNotFound);
        await _transactionRepository.DidNotReceive().UpdateAsync(Arg.Any<Transaction>());
    }

    [Fact]
    public async Task Handle_WhenTransactionBelongsToAnotherUser_ReturnsForbidden()
    {
        var transaction = TransactionMother.Default();

        _transactionRepository.GetByIdWithStatementAsync(transaction.Id).Returns(transaction);

        var result = await _handler.Handle(
            new AssignCategoryCommand(transaction.Id, Guid.NewGuid(), null),
            CancellationToken.None);

        result.Should().Be(AssignCategoryResult.Forbidden);
    }

    [Fact]
    public async Task Handle_WhenCategoryBelongsToAnotherUser_ReturnsCategoryNotFound()
    {
        var userId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var transaction = TransactionMother.OwnedBy(userId);
        var category = CategoryMother.Default(id: categoryId); // outro userId

        _transactionRepository.GetByIdWithStatementAsync(transaction.Id).Returns(transaction);
        _categoryRepository.GetByIdAsync(categoryId).Returns(category);

        var result = await _handler.Handle(
            new AssignCategoryCommand(transaction.Id, userId, categoryId),
            CancellationToken.None);

        result.Should().Be(AssignCategoryResult.CategoryNotFound);
    }

    [Fact]
    public async Task Handle_WhenCategoryDoesNotExist_ReturnsCategoryNotFound()
    {
        var userId = Guid.NewGuid();
        var transaction = TransactionMother.OwnedBy(userId);

        _transactionRepository.GetByIdWithStatementAsync(transaction.Id).Returns(transaction);
        _categoryRepository.GetByIdAsync(Arg.Any<Guid>()).Returns((Category?)null);

        var result = await _handler.Handle(
            new AssignCategoryCommand(transaction.Id, userId, Guid.NewGuid()),
            CancellationToken.None);

        result.Should().Be(AssignCategoryResult.CategoryNotFound);
    }
}
