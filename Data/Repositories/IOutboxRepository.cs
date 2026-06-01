using RabbitDemo.Contracts.Messages;
using RabbitDemo.Data.Entities;

namespace RabbitDemo.Data.Repositories;

public interface IOutboxRepository
{
    Task SaveAsync(
        TicketCreated ticket,
        CancellationToken cancellationToken = default);

    Task<List<OutboxMessage>> GetPendingAsync(
        CancellationToken cancellationToken = default);

    Task MarkProcessedAsync(
        int id,
        CancellationToken cancellationToken = default);
}