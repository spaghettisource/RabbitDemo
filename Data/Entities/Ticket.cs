namespace RabbitDemo.Data.Entities;

public class Ticket
{
    public int Id { get; set; }

    public int EventId { get; set; }

    public string Customer { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
}