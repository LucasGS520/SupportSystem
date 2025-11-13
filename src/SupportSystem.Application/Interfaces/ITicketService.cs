using SupportSystem.Application.DTOs;
using SupportSystem.Domain.Entities;

namespace SupportSystem.Application.Interfaces;

public interface ITicketService
{
    Task<TicketDto> CreateTicketAsync(CreateTicketDto createTicketDto);
    Task<TicketDto?> GetTicketByIdAsync(int id);
    Task<IEnumerable<TicketDto>> GetAllTicketsAsync();
    Task<IEnumerable<TicketDto>> GetTicketsByCustomerIdAsync(int customerId);
    Task<TicketDto> UpdateTicketStatusAsync(int id, Domain.Enums.TicketStatus status);
    Task<bool> AssignTicketAsync(int ticketId, int userId);
    Task<bool> AddCommentAsync(int ticketId, string content, string authorName, string authorEmail, bool isInternal);
}
