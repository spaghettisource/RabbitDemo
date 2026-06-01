namespace RabbitDemo.Data.Entities;

public class OutboxMessage
{
    public int Id { get; set; }

    public string Type { get; set; } = string.Empty;

    public string Payload { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public DateTime? ProcessedAt { get; set; }
}