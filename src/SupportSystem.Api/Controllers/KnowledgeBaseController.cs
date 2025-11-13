using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SupportSystem.Domain.Entities;
using SupportSystem.Domain.Enums;
using SupportSystem.Infrastructure.Data;

namespace SupportSystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class KnowledgeBaseController : ControllerBase
{
    private readonly SupportSystemDbContext _context;
    private readonly ILogger<KnowledgeBaseController> _logger;

    public KnowledgeBaseController(SupportSystemDbContext context, ILogger<KnowledgeBaseController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Get all published knowledge base articles
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<KnowledgeBaseArticle>>> GetArticles(
        [FromQuery] TicketCategory? category = null,
        [FromQuery] string? searchTerm = null)
    {
        var query = _context.KnowledgeBaseArticles.Where(a => a.IsPublished);

        if (category.HasValue)
        {
            query = query.Where(a => a.Category == category.Value);
        }

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(a => 
                a.Title.Contains(searchTerm) || 
                a.Content.Contains(searchTerm));
        }

        var articles = await query
            .OrderByDescending(a => a.ViewCount)
            .ToListAsync();

        return Ok(articles);
    }

    /// <summary>
    /// Get article by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<KnowledgeBaseArticle>> GetArticle(int id)
    {
        var article = await _context.KnowledgeBaseArticles.FindAsync(id);
        if (article == null || !article.IsPublished)
            return NotFound();

        // Increment view count
        article.ViewCount++;
        await _context.SaveChangesAsync();

        return Ok(article);
    }

    /// <summary>
    /// Create a new knowledge base article
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<KnowledgeBaseArticle>> CreateArticle([FromBody] CreateArticleRequest request)
    {
        var article = new KnowledgeBaseArticle
        {
            Title = request.Title,
            Content = request.Content,
            Category = request.Category,
            Tags = request.Tags ?? Array.Empty<string>(),
            IsPublished = request.IsPublished,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = request.CreatedBy
        };

        _context.KnowledgeBaseArticles.Add(article);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetArticle), new { id = article.Id }, article);
    }

    /// <summary>
    /// Mark article as helpful
    /// </summary>
    [HttpPost("{id}/helpful")]
    public async Task<ActionResult> MarkAsHelpful(int id)
    {
        var article = await _context.KnowledgeBaseArticles.FindAsync(id);
        if (article == null)
            return NotFound();

        article.HelpfulCount++;
        await _context.SaveChangesAsync();

        return Ok();
    }

    /// <summary>
    /// Get suggested articles based on ticket description
    /// </summary>
    [HttpPost("suggest")]
    public async Task<ActionResult<IEnumerable<KnowledgeBaseArticle>>> SuggestArticles([FromBody] SuggestArticlesRequest request)
    {
        // Simple keyword-based suggestion
        var keywords = request.Description
            .ToLowerInvariant()
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Where(w => w.Length > 3)
            .Take(5);

        var articles = await _context.KnowledgeBaseArticles
            .Where(a => a.IsPublished)
            .ToListAsync();

        var relevantArticles = articles
            .Where(a => keywords.Any(k => 
                a.Title.ToLowerInvariant().Contains(k) || 
                a.Content.ToLowerInvariant().Contains(k)))
            .OrderByDescending(a => a.HelpfulCount)
            .Take(5)
            .ToList();

        return Ok(relevantArticles);
    }
}

public class CreateArticleRequest
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public TicketCategory Category { get; set; }
    public string[]? Tags { get; set; }
    public bool IsPublished { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
}

public class SuggestArticlesRequest
{
    public string Description { get; set; } = string.Empty;
}
