using System.Text;
using RabbitDemo.Data.Repositories;
using RabbitMQ.Client;

namespace OrderWorker;

public class OutboxPublisherWorker(
    ILogger<OutboxPublisherWorker> logger,
    IOutboxRepository outboxRepository,
    IConnection connection)
    : BackgroundService
{
    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        logger.LogInformation(
            "OutboxPublisherWorker started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var messages =
                    await outboxRepository.GetPendingAsync(
                        stoppingToken);

                if (messages.Count > 0)
                {
                    await using var channel =
                        await connection.CreateChannelAsync(
                            cancellationToken: stoppingToken);

                    foreach (var message in messages)
                    {
                        var body =
                            Encoding.UTF8.GetBytes(
                                message.Payload);

                        await channel.BasicPublishAsync(
                            exchange: string.Empty,
                            routingKey: "ticket-orders",
                            body: body,
                            cancellationToken: stoppingToken);

                        await outboxRepository.MarkProcessedAsync(
                            message.Id,
                            stoppingToken);

                        logger.LogInformation(
                            "Published outbox message {Id}",
                            message.Id);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Outbox publisher failed");
            }

            await Task.Delay(
                TimeSpan.FromSeconds(5),
                stoppingToken);
        }
    }
}