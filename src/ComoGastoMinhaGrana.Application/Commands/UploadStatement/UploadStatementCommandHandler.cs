using ComoGastoMinhaGrana.Application.Common.Interfaces;
using ComoGastoMinhaGrana.Application.Common.Messages;
using ComoGastoMinhaGrana.Domain.Entities;
using ComoGastoMinhaGrana.Domain.Enums;
using MediatR;

namespace ComoGastoMinhaGrana.Application.Commands.UploadStatement;

public class UploadStatementCommandHandler : IRequestHandler<UploadStatementCommand, Guid>
{
    private readonly IDocumentExtractorFactory _extractorFactory;
    private readonly ISanitizerService _sanitizer;
    private readonly IFinancialStatementRepository _statementRepository;
    private readonly IMessagePublisher _messagePublisher;

    public UploadStatementCommandHandler(
        IDocumentExtractorFactory extractorFactory,
        ISanitizerService sanitizer,
        IFinancialStatementRepository statementRepository,
        IMessagePublisher messagePublisher)
    {
        _extractorFactory = extractorFactory;
        _sanitizer = sanitizer;
        _statementRepository = statementRepository;
        _messagePublisher = messagePublisher;
    }

    public async Task<Guid> Handle(UploadStatementCommand request, CancellationToken cancellationToken)
    {
        var extension = Path.GetExtension(request.FileName);

        var rawText = await _extractorFactory.ExtractTextAsync(request.FileStream, request.FileName);
        var sanitizedText = _sanitizer.Sanitize(rawText);

        var statement = new FinancialStatement
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            FileName = request.FileName,
            FileExtension = extension.ToLowerInvariant(),
            UploadDate = DateTime.UtcNow,
            Status = StatementStatus.Pending,
        };

        await _statementRepository.AddAsync(statement);

        await _messagePublisher.PublishAsync(new ProcessStatementMessage(
            statement.Id,
            request.UserId,
            sanitizedText,
            request.FileName), cancellationToken);

        return statement.Id;
    }
}
