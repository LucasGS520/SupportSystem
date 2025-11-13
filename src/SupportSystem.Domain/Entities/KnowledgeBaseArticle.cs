using SupportSystem.Domain.Enums;

namespace SupportSystem.Domain.Entities;

public class KnowledgeBaseArticle
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public TicketCategory Category { get; set; }
    public string[] Tags { get; set; } = Array.Empty<string>();
    public int ViewCount { get; set; }
    public int HelpfulCount { get; set; }
    public bool IsPublished { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
}
