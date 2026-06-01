using Microsoft.AspNetCore.Mvc;
using RabbitDemo.Contracts.Messages;
using RabbitDemo.Data.Repositories;

namespace OrderApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TicketsController : ControllerBase
{
    private readonly IOutboxRepository _outboxRepository;

    public TicketsController(
        IOutboxRepository outboxRepository)
    {
        _outboxRepository = outboxRepository;
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        TicketCreated ticket)
    {
        await _outboxRepository.SaveAsync(ticket);

        return Ok(new
        {
            Message = "Ticket saved to outbox"
        });
    }
}