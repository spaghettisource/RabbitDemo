using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using OrderWorker.Telemetry;
using RabbitDemo.Contracts.Configuration;
using RabbitDemo.Contracts.Messages;
using RabbitDemo.Data.Repositories;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace OrderWorker;

public class Worker(
    ILogger<Worker> logger,
    ITicketRepository repository,
    IOptions<RabbitMqOptions> rabbitOptions)
    : BackgroundService
{
    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory
        {
            HostName = rabbitOptions.Value.Host
        };

        var connection =
            await factory.CreateConnectionAsync(stoppingToken);

        var channel =
            await connection.CreateChannelAsync(
                cancellationToken: stoppingToken);

        await channel.QueueDeclareAsync(
            queue: "ticket-orders",
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(channel);

        consumer.ReceivedAsync += async (_, args) =>
        {
            try
            {
                var json =
                    Encoding.UTF8.GetString(args.Body.ToArray());

                var ticket =
                    JsonSerializer.Deserialize<TicketCreated>(json);

                string? traceParent = null;

                if (args.BasicProperties.Headers is not null &&
                    args.BasicProperties.Headers.TryGetValue(
                        "traceparent",
                        out var traceParentBytes))
                {
                    traceParent =
                        Encoding.UTF8.GetString(
                            (byte[])traceParentBytes);
                }

                ActivityContext parentContext = default;

                if (!string.IsNullOrEmpty(traceParent))
                {
                    ActivityContext.TryParse(
                        traceParent,
                        null,
                        out parentContext);
                }

                if (ticket is not null)
                {
                    using var activity =
                        WorkerActivitySource.Source.StartActivity(
                            "Process Ticket",
                            ActivityKind.Consumer,
                            parentContext);

                    activity?.SetTag(
                        "ticket.event_id",
                        ticket.EventId);

                    activity?.SetTag(
                        "ticket.customer",
                        ticket.Customer);

                    await repository.SaveAsync(
                        ticket,
                        stoppingToken);

                    logger.LogInformation(
                        "Ticket saved. EventId: {EventId}, Customer: {Customer}",
                        ticket.EventId,
                        ticket.Customer);
                }

                await channel.BasicAckAsync(
                    args.DeliveryTag,
                    false,
                    stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Error processing message");

                await channel.BasicNackAsync(
                    args.DeliveryTag,
                    false,
                    true,
                    stoppingToken);
            }
        };

        await channel.BasicConsumeAsync(
            queue: "ticket-orders",
            autoAck: false,
            consumer: consumer,
            cancellationToken: stoppingToken);

        logger.LogInformation(
            "TicketWorker started and listening on ticket-orders");

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(
                1000,
                stoppingToken);
        }
    }
}