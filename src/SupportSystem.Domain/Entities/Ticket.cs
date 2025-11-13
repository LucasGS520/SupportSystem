using SupportSystem.Domain.Enums;

namespace SupportSystem.Domain.Entities;

public class Ticket
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public TicketStatus Status { get; set; }
    public TicketPriority Priority { get; set; }
    public TicketCategory Category { get; set; }
    public int CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;
    public int? AssignedToId { get; set; }
    public User? AssignedTo { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public string? Resolution { get; set; }
    public ICollection<TicketComment> Comments { get; set; } = new List<TicketComment>();
    
    // LGPD compliance fields
    public bool PersonalDataProcessingConsent { get; set; }
    public DateTime? DataRetentionExpiresAt { get; set; }
}
