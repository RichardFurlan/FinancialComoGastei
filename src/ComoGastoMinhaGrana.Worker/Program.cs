using ComoGastoMinhaGrana.Application;
using ComoGastoMinhaGrana.Infrastructure;
using ComoGastoMinhaGrana.Worker.Consumers;
using MassTransit;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration, builder.Environment);

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<ProcessStatementConsumer>(cfg =>
    {
        cfg.UseMessageRetry(r =>
            r.Exponential(3, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(60), TimeSpan.FromSeconds(10)));
    });

    x.UsingRabbitMq((ctx, cfg) =>
    {
        var rabbit = builder.Configuration.GetSection("RabbitMQ");
        cfg.Host(rabbit["Host"] ?? "localhost", h =>
        {
            h.Username(rabbit["Username"] ?? "guest");
            h.Password(rabbit["Password"] ?? "guest");
        });
        cfg.ConfigureEndpoints(ctx);
    });
});

var host = builder.Build();
host.Run();
