using Microsoft.AspNetCore.Mvc;
using OrderApi.Services;
using RabbitDemo.Contracts.Messages;

namespace OrderApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TicketsController : ControllerBase
{
    private readonly IMessagePublisher _publisher;

    public TicketsController(IMessagePublisher publisher)
    {
        _publisher = publisher;
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        TicketCreated ticket)
    {
        await _publisher.PublishAsync(ticket);

        return Ok(new
        {
            Message = "Ticket published"
        });
    }
}