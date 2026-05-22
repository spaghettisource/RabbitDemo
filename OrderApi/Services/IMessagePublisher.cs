
using RabbitDemo.Contracts.Messages;

namespace OrderApi.Services;

public interface IMessagePublisher
{
    Task PublishAsync(TicketCreated ticket);
}