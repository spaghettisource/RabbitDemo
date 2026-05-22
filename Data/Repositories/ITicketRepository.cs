using RabbitDemo.Contracts.Messages;

namespace RabbitDemo.Data.Repositories;

public interface ITicketRepository
{
    Task SaveAsync(
        TicketCreated ticket,
        CancellationToken cancellationToken = default);
}