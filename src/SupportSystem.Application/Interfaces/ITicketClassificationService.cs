using SupportSystem.Application.DTOs;

namespace SupportSystem.Application.Interfaces;

public interface ITicketClassificationService
{
    Task<TicketClassificationResult> ClassifyTicketAsync(string title, string description);
}
