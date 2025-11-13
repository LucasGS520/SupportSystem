using SupportSystem.Domain.Enums;

namespace SupportSystem.Application.DTOs;

public class TicketClassificationResult
{
    public TicketCategory PredictedCategory { get; set; }
    public TicketPriority PredictedPriority { get; set; }
    public float CategoryConfidence { get; set; }
    public float PriorityConfidence { get; set; }
}
