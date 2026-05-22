using Dapper;
using Npgsql;
using RabbitDemo.Contracts.Messages;

namespace RabbitDemo.Data.Repositories;

public class TicketRepository : ITicketRepository
{
    private readonly string _connectionString;

    public TicketRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task SaveAsync(
        TicketCreated ticket,
        CancellationToken cancellationToken = default)
    {
        await using var connection =
            new NpgsqlConnection(_connectionString);

        await connection.OpenAsync(cancellationToken);

        const string sql = """
            INSERT INTO tickets
            (
                event_id,
                customer
            )
            VALUES
            (
                @EventId,
                @Customer
            );
            """;

        await connection.ExecuteAsync(
            sql,
            new
            {
                ticket.EventId,
                ticket.Customer
            });
    }
}