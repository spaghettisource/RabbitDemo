using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using RabbitDemo.Contracts.Configuration;
using RabbitDemo.Contracts.Messages;
using RabbitMQ.Client;

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

        var json =
            JsonSerializer.Serialize(ticket);

        var body =
            Encoding.UTF8.GetBytes(json);

        var properties =
            new BasicProperties();

        properties.Headers =
            new Dictionary<string, object>();

        if (Activity.Current?.Id is not null)
        {
            properties.Headers["traceparent"] =
                Encoding.UTF8.GetBytes(
                    Activity.Current.Id);
        }

        await channel.BasicPublishAsync(
            exchange: string.Empty,
            routingKey: "ticket-orders",
            mandatory: false,
            basicProperties: properties,
            body: body);
    }
}