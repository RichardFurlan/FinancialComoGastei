using ComoGastoMinhaGrana.Application.Common.Interfaces;

namespace ComoGastoMinhaGrana.Infrastructure.Services;

// Usado em desenvolvimento para não precisar do RabbitMQ rodando localmente
public class NoOpMessagePublisher : IMessagePublisher
{
    public Task PublishAsync<T>(T message, CancellationToken cancellationToken = default) where T : class
        => Task.CompletedTask;
}
