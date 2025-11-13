using Microsoft.AspNetCore.Mvc;
using SupportSystem.Application.DTOs;
using SupportSystem.Application.Interfaces;
using SupportSystem.Domain.Enums;

namespace SupportSystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TicketsController : ControllerBase
{
    private readonly ITicketService _ticketService;
    private readonly ILogger<TicketsController> _logger;

    public TicketsController(ITicketService ticketService, ILogger<TicketsController> logger)
    {
        _ticketService = ticketService;
        _logger = logger;
    }

    /// <summary>
    /// Create a new support ticket
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<TicketDto>> CreateTicket([FromBody] CreateTicketDto createTicketDto)
    {
        try
        {
            var ticket = await _ticketService.CreateTicketAsync(createTicketDto);
            return CreatedAtAction(nameof(GetTicket), new { id = ticket.Id }, ticket);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating ticket");
            return StatusCode(500, "An error occurred while creating the ticket");
        }
    }

    /// <summary>
    /// Get a ticket by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<TicketDto>> GetTicket(int id)
    {
        var ticket = await _ticketService.GetTicketByIdAsync(id);
        if (ticket == null)
            return NotFound();

        return Ok(ticket);
    }

    /// <summary>
    /// Get all tickets
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TicketDto>>> GetAllTickets()
    {
        var tickets = await _ticketService.GetAllTicketsAsync();
        return Ok(tickets);
    }

    /// <summary>
    /// Get tickets by customer ID
    /// </summary>
    [HttpGet("customer/{customerId}")]
    public async Task<ActionResult<IEnumerable<TicketDto>>> GetTicketsByCustomer(int customerId)
    {
        var tickets = await _ticketService.GetTicketsByCustomerIdAsync(customerId);
        return Ok(tickets);
    }

    /// <summary>
    /// Update ticket status
    /// </summary>
    [HttpPatch("{id}/status")]
    public async Task<ActionResult<TicketDto>> UpdateTicketStatus(int id, [FromBody] TicketStatus status)
    {
        try
        {
            var ticket = await _ticketService.UpdateTicketStatusAsync(id, status);
            return Ok(ticket);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
    }

    /// <summary>
    /// Assign ticket to a user
    /// </summary>
    [HttpPost("{ticketId}/assign/{userId}")]
    public async Task<ActionResult> AssignTicket(int ticketId, int userId)
    {
        var result = await _ticketService.AssignTicketAsync(ticketId, userId);
        if (!result)
            return NotFound("Ticket or user not found");

        return Ok();
    }

    /// <summary>
    /// Add a comment to a ticket
    /// </summary>
    [HttpPost("{ticketId}/comments")]
    public async Task<ActionResult> AddComment(
        int ticketId, 
        [FromBody] AddCommentRequest request)
    {
        var result = await _ticketService.AddCommentAsync(
            ticketId, 
            request.Content, 
            request.AuthorName, 
            request.AuthorEmail, 
            request.IsInternal);

        if (!result)
            return NotFound("Ticket not found");

        return Ok();
    }
}

public class AddCommentRequest
{
    public string Content { get; set; } = string.Empty;
    public string AuthorName { get; set; } = string.Empty;
    public string AuthorEmail { get; set; } = string.Empty;
    public bool IsInternal { get; set; }
}
