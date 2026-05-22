namespace RabbitDemo.Contracts.Messages;

public class TicketCreated
{
    public int EventId { get; set; }

    public string Customer { get; set; } = string.Empty;
}