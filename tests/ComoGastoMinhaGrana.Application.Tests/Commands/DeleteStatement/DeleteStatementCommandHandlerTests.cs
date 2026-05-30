using ComoGastoMinhaGrana.Application.Commands.DeleteStatement;
using ComoGastoMinhaGrana.Application.Common.Interfaces;
using ComoGastoMinhaGrana.Application.Tests.Common.Mothers;
using ComoGastoMinhaGrana.Domain.Entities;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace ComoGastoMinhaGrana.Application.Tests.Commands.DeleteStatement;

public class DeleteStatementCommandHandlerTests
{
    private readonly IFinancialStatementRepository _statementRepository = Substitute.For<IFinancialStatementRepository>();
    private readonly IAnalysisCacheService _cache = Substitute.For<IAnalysisCacheService>();
    private readonly ILogger<DeleteStatementCommandHandler> _logger = Substitute.For<ILogger<DeleteStatementCommandHandler>>();
    private readonly DeleteStatementCommandHandler _handler;

    public DeleteStatementCommandHandlerTests()
    {
        _handler = new DeleteStatementCommandHandler(_statementRepository, _cache, _logger);
    }

    [Fact]
    public async Task Handle_WhenOwner_DeletesStatementAndEvictsCache()
    {
        var userId = Guid.NewGuid();
        var statement = FinancialStatementMother.OwnedBy(userId);

        _statementRepository.GetByIdAndUserIdAsync(statement.Id, userId).Returns(statement);

        var result = await _handler.Handle(
            new DeleteStatementCommand(statement.Id, userId),
            CancellationToken.None);

        result.Should().Be(DeleteStatementError.None);
        await _statementRepository.Received(1).DeleteAsync(statement);
        await _cache.Received(1).RemoveAsync(statement.Id);
    }

    [Fact]
    public async Task Handle_WhenStatementNotFound_ReturnsNotFound()
    {
        _statementRepository.GetByIdAndUserIdAsync(Arg.Any<Guid>(), Arg.Any<Guid>()).Returns((FinancialStatement?)null);

        var result = await _handler.Handle(
            new DeleteStatementCommand(Guid.NewGuid(), Guid.NewGuid()),
            CancellationToken.None);

        result.Should().Be(DeleteStatementError.NotFound);
        await _statementRepository.DidNotReceive().DeleteAsync(Arg.Any<FinancialStatement>());
        await _cache.DidNotReceive().RemoveAsync(Arg.Any<Guid>());
    }

    [Fact]
    public async Task Handle_WhenStatusIsProcessing_BlocksDelete()
    {
        var userId = Guid.NewGuid();
        var statement = FinancialStatementMother.Processing(userId);

        _statementRepository.GetByIdAndUserIdAsync(statement.Id, userId).Returns(statement);

        var result = await _handler.Handle(
            new DeleteStatementCommand(statement.Id, userId),
            CancellationToken.None);

        result.Should().Be(DeleteStatementError.Processing);
        await _statementRepository.DidNotReceive().DeleteAsync(Arg.Any<FinancialStatement>());
        await _cache.DidNotReceive().RemoveAsync(Arg.Any<Guid>());
    }
}
