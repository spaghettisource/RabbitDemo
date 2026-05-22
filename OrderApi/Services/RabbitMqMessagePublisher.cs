using Microsoft.Extensions.Options;
using OrderWorker;
using RabbitDemo.Contracts.Messages;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace OrderApi.Services;

public class RabbitMqMessagePublisher(
    IOptions<RabbitMqOptions> rabbitOptions)
    : IMessagePublisher
{
    public async Task PublishAsync(TicketCreated ticket)
    {
        var factory = new ConnectionFactory
        {
            HostName = rabbitOptions.Value.Host
        };

        await using var connection =
            await factory.CreateConnectionAsync();

        await using var channel =
            await connection.CreateChannelAsync();

        await channel.QueueDeclareAsync(
            queue: "ticket-orders",
            durable: true,
            exclusive: false,
            autoDelete: false);

        var json = JsonSerializer.Serialize(ticket);

        var body = Encoding.UTF8.GetBytes(json);

        await channel.BasicPublishAsync(
            exchange: string.Empty,
            routingKey: "ticket-orders",
            body: body);
    }
}