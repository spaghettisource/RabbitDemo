using OpenTelemetry.Exporter;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OrderWorker;
using RabbitDemo.Contracts.Configuration;
using RabbitDemo.Data.Repositories;
using RabbitMQ.Client;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<RabbitMqOptions>(
    builder.Configuration.GetSection("RabbitMq"));

var connectionString =
    builder.Configuration.GetConnectionString("TicketDb")
    ?? throw new InvalidOperationException(
        "Connection string TicketDb not found.");

builder.Services.AddSingleton<ITicketRepository>(
    _ => new TicketRepository(connectionString));

builder.Services.AddSingleton<IOutboxRepository>(
    _ => new OutboxRepository(connectionString));

builder.Services.AddSingleton<IConnection>(_ =>
{
    var factory = new ConnectionFactory
    {
        HostName =
            builder.Configuration["RabbitMq:Host"]
    };

    return factory.CreateConnectionAsync()
        .GetAwaiter()
        .GetResult();
});

builder.Services.AddOpenTelemetry()
    .WithTracing(tracing =>
    {
        tracing
            .SetResourceBuilder(
                ResourceBuilder.CreateDefault()
                    .AddService("OrderWorker"))
            .AddSource("OrderWorker")
            .AddConsoleExporter()
            .AddOtlpExporter(options =>
            {
                options.Endpoint =
                    new Uri("http://tempo:4318/v1/traces");

                options.Protocol =
                    OtlpExportProtocol.HttpProtobuf;
            });
    });

builder.Services.AddHostedService<Worker>();
builder.Services.AddHostedService<OutboxPublisherWorker>();

var host = builder.Build();

host.Run();