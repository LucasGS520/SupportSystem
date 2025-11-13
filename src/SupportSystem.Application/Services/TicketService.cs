using Microsoft.EntityFrameworkCore;
using SupportSystem.Application.DTOs;
using SupportSystem.Application.Interfaces;
using SupportSystem.Domain.Entities;
using SupportSystem.Domain.Enums;
using SupportSystem.Infrastructure.Data;

namespace SupportSystem.Application.Services;

public class TicketService : ITicketService
{
    private readonly SupportSystemDbContext _context;
    private readonly ITicketClassificationService _classificationService;

    public TicketService(SupportSystemDbContext context, ITicketClassificationService classificationService)
    {
        _context = context;
        _classificationService = classificationService;
    }

    public async Task<TicketDto> CreateTicketAsync(CreateTicketDto createTicketDto)
    {
        // Auto-classify ticket using AI
        var classification = await _classificationService.ClassifyTicketAsync(
            createTicketDto.Title, 
            createTicketDto.Description);

        var ticket = new Ticket
        {
            Title = createTicketDto.Title,
            Description = createTicketDto.Description,
            CustomerId = createTicketDto.CustomerId,
            Category = createTicketDto.Category ?? classification.PredictedCategory,
            Priority = classification.PredictedPriority,
            Status = TicketStatus.Open,
            CreatedAt = DateTime.UtcNow,
            PersonalDataProcessingConsent = createTicketDto.PersonalDataProcessingConsent,
            DataRetentionExpiresAt = DateTime.UtcNow.AddYears(5) // LGPD compliance: 5-year retention
        };

        _context.Tickets.Add(ticket);
        await _context.SaveChangesAsync();

        return await GetTicketByIdAsync(ticket.Id) ?? throw new InvalidOperationException("Failed to create ticket");
    }

    public async Task<TicketDto?> GetTicketByIdAsync(int id)
    {
        var ticket = await _context.Tickets
            .Include(t => t.Customer)
            .Include(t => t.AssignedTo)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (ticket == null)
            return null;

        return MapToDto(ticket);
    }

    public async Task<IEnumerable<TicketDto>> GetAllTicketsAsync()
    {
        var tickets = await _context.Tickets
            .Include(t => t.Customer)
            .Include(t => t.AssignedTo)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

        return tickets.Select(MapToDto);
    }

    public async Task<IEnumerable<TicketDto>> GetTicketsByCustomerIdAsync(int customerId)
    {
        var tickets = await _context.Tickets
            .Include(t => t.Customer)
            .Include(t => t.AssignedTo)
            .Where(t => t.CustomerId == customerId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

        return tickets.Select(MapToDto);
    }

    public async Task<TicketDto> UpdateTicketStatusAsync(int id, TicketStatus status)
    {
        var ticket = await _context.Tickets.FindAsync(id);
        if (ticket == null)
            throw new InvalidOperationException("Ticket not found");

        ticket.Status = status;
        ticket.UpdatedAt = DateTime.UtcNow;

        if (status == TicketStatus.Resolved || status == TicketStatus.Closed)
        {
            ticket.ResolvedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        return await GetTicketByIdAsync(id) ?? throw new InvalidOperationException("Failed to update ticket");
    }

    public async Task<bool> AssignTicketAsync(int ticketId, int userId)
    {
        var ticket = await _context.Tickets.FindAsync(ticketId);
        if (ticket == null)
            return false;

        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            return false;

        ticket.AssignedToId = userId;
        ticket.Status = TicketStatus.InProgress;
        ticket.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> AddCommentAsync(int ticketId, string content, string authorName, string authorEmail, bool isInternal)
    {
        var ticket = await _context.Tickets.FindAsync(ticketId);
        if (ticket == null)
            return false;

        var comment = new TicketComment
        {
            TicketId = ticketId,
            Content = content,
            AuthorName = authorName,
            AuthorEmail = authorEmail,
            IsInternal = isInternal,
            CreatedAt = DateTime.UtcNow
        };

        _context.TicketComments.Add(comment);
        
        ticket.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        return true;
    }

    private static TicketDto MapToDto(Ticket ticket)
    {
        return new TicketDto
        {
            Id = ticket.Id,
            Title = ticket.Title,
            Description = ticket.Description,
            Status = ticket.Status,
            Priority = ticket.Priority,
            Category = ticket.Category,
            CustomerId = ticket.CustomerId,
            CustomerName = ticket.Customer?.Name ?? string.Empty,
            AssignedToId = ticket.AssignedToId,
            AssignedToName = ticket.AssignedTo?.FullName,
            CreatedAt = ticket.CreatedAt,
            UpdatedAt = ticket.UpdatedAt,
            ResolvedAt = ticket.ResolvedAt,
            Resolution = ticket.Resolution
        };
    }
}
