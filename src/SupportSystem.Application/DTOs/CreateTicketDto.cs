using SupportSystem.Domain.Enums;

namespace SupportSystem.Application.DTOs;

public class CreateTicketDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int CustomerId { get; set; }
    public TicketCategory? Category { get; set; }
    public bool PersonalDataProcessingConsent { get; set; }
}
