using MediatR;

namespace ComoGastoMinhaGrana.Application.Commands.UploadStatement;

public record UploadStatementCommand : IRequest<Guid>
{
    public Guid UserId { get; init; }
    public string FileName { get; init; } = string.Empty;
    public Stream FileStream { get; init; } = Stream.Null;
}
