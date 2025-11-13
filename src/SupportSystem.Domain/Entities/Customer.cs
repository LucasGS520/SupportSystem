namespace SupportSystem.Domain.Entities;

public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Company { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
    
    // LGPD compliance fields
    public bool DataProcessingConsent { get; set; }
    public DateTime? ConsentDate { get; set; }
    public bool DataDeletionRequested { get; set; }
    public DateTime? DataDeletionRequestDate { get; set; }
}
