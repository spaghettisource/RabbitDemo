using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OrderApi.Services;
using RabbitDemo.Contracts.Configuration;
using RabbitDemo.Data.Repositories;
using RabbitMQ.Client;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Trace);

builder.Services.AddControllers();

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

builder.Services.AddHealthChecks()
    .AddRabbitMQ();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<RabbitMqOptions>(
    builder.Configuration.GetSection("RabbitMq"));

var connectionString =
    builder.Configuration.GetConnectionString("TicketDb")
    ?? throw new InvalidOperationException(
        "Connection string TicketDb not found.");

builder.Services.AddSingleton<IOutboxRepository>(
    _ => new OutboxRepository(connectionString));

builder.Services.AddScoped<IMessagePublisher,
    RabbitMqMessagePublisher>();

builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource =>
        resource.AddService("OrderApi"))
    .WithTracing(tracing =>
    {
        tracing
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddConsoleExporter()
            .AddOtlpExporter(options =>
            {
                options.Endpoint =
                    new Uri("http://tempo:4318/v1/traces");

                options.Protocol =
                    OtlpExportProtocol.HttpProtobuf;
            });
    })
    .WithMetrics(metrics =>
    {
        metrics
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddPrometheusExporter();
    });

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapHealthChecks("/health");

app.MapPrometheusScrapingEndpoint();

app.Run();