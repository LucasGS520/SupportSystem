using Microsoft.AspNetCore.Mvc;
using SupportSystem.Application.DTOs;
using SupportSystem.Application.Interfaces;

namespace SupportSystem.Api.Controllers;

// Controlador HTTP responsável pelas operações de tickets.
[ApiController]
[Route("api/[controller]")]
public class TicketsController : ControllerBase
{
    // Serviço de domínio que concentra as regras de ticket.
    private readonly ITicketService _ticketService;

    // Construtor que injeta o serviço de tickets.
    public TicketsController(ITicketService ticketService)
    {
        _ticketService = ticketService;
    }

    // Retorna todos os tickets disponíveis.
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<TicketResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<TicketResponse>>> GetTickets(CancellationToken cancellationToken)
    {
        var tickets = await _ticketService.GetAllAsync(cancellationToken);
        return Ok(tickets);
    }

    // Busca um ticket específico pelo identificador.
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(TicketResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TicketResponse>> GetTicket(int id, CancellationToken cancellationToken)
    {
        var ticket = await _ticketService.GetByIdAsync(id, cancellationToken);
        if (ticket is null)
        {
            return NotFound();
        }

        return Ok(ticket);
    }

    // Cria um novo ticket a partir do payload enviado.
    [HttpPost]
    [ProducesResponseType(typeof(TicketResponse), StatusCodes.Status201Created)]
    public async Task<ActionResult<TicketResponse>> CreateTicket(
        [FromBody] CreateTicketRequest request,
        CancellationToken cancellationToken)
    {
        var created = await _ticketService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetTicket), new { id = created.Id }, created);
    }

    // Atualiza os dados de um ticket existente.
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateTicket(
        int id,
        [FromBody] UpdateTicketRequest request,
        CancellationToken cancellationToken)
    {
        var updated = await _ticketService.UpdateAsync(id, request, cancellationToken);
        return updated ? NoContent() : NotFound();
    }

    // Remove um ticket definitivamente.
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteTicket(int id, CancellationToken cancellationToken)
    {
        var deleted = await _ticketService.DeleteAsync(id, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }
}
