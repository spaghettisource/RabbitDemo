using Dapper;
using Npgsql;
using RabbitDemo.Contracts.Messages;
using RabbitDemo.Data.Entities;
using System.Text.Json;

namespace RabbitDemo.Data.Repositories;

public class OutboxRepository : IOutboxRepository
{
    private readonly string _connectionString;

    public OutboxRepository(string connectionString)
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
            INSERT INTO outbox_messages
            (
                type,
                payload
            )
            VALUES
            (
                @Type,
                @Payload
            );
            """;

        await connection.ExecuteAsync(
            sql,
            new
            {
                Type = nameof(TicketCreated),
                Payload = JsonSerializer.Serialize(ticket)
            });
    }

    public async Task<List<OutboxMessage>> GetPendingAsync(
    CancellationToken cancellationToken = default)
    {
        await using var connection =
            new NpgsqlConnection(_connectionString);

        await connection.OpenAsync(cancellationToken);

        const string sql = """
        SELECT
            id,
            type,
            payload,
            created_at,
            processed_at
        FROM outbox_messages
        WHERE processed_at IS NULL
        ORDER BY id;
        """;

        var result =
            await connection.QueryAsync<OutboxMessage>(sql);

        return result.ToList();
    }

    public async Task MarkProcessedAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        await using var connection =
            new NpgsqlConnection(_connectionString);

        await connection.OpenAsync(cancellationToken);

        const string sql = """
        UPDATE outbox_messages
        SET processed_at = NOW()
        WHERE id = @Id;
        """;

        await connection.ExecuteAsync(
            sql,
            new { Id = id });
    }
}