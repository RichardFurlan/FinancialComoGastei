namespace ComoGastoMinhaGrana.Application.Common.Messages;

public record ProcessStatementMessage(
    Guid StatementId,
    Guid UserId,
    string SanitizedText,
    string FileName);
