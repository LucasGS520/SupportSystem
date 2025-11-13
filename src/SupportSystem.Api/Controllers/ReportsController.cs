using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SupportSystem.Domain.Enums;
using SupportSystem.Infrastructure.Data;

namespace SupportSystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportsController : ControllerBase
{
    private readonly SupportSystemDbContext _context;
    private readonly ILogger<ReportsController> _logger;

    public ReportsController(SupportSystemDbContext context, ILogger<ReportsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Get KPIs and performance indicators
    /// </summary>
    [HttpGet("kpis")]
    public async Task<ActionResult<KpiReport>> GetKpis(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        startDate ??= DateTime.UtcNow.AddMonths(-1);
        endDate ??= DateTime.UtcNow;

        var tickets = await _context.Tickets
            .Where(t => t.CreatedAt >= startDate && t.CreatedAt <= endDate)
            .ToListAsync();

        var resolvedTickets = tickets.Where(t => t.Status == TicketStatus.Resolved || t.Status == TicketStatus.Closed).ToList();

        var report = new KpiReport
        {
            Period = $"{startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}",
            TotalTickets = tickets.Count,
            OpenTickets = tickets.Count(t => t.Status == TicketStatus.Open),
            InProgressTickets = tickets.Count(t => t.Status == TicketStatus.InProgress),
            ResolvedTickets = resolvedTickets.Count,
            CriticalTickets = tickets.Count(t => t.Priority == TicketPriority.Critical),
            AverageResolutionTimeHours = resolvedTickets
                .Where(t => t.ResolvedAt.HasValue)
                .Select(t => (t.ResolvedAt!.Value - t.CreatedAt).TotalHours)
                .DefaultIfEmpty(0)
                .Average(),
            TicketsByCategory = tickets
                .GroupBy(t => t.Category)
                .ToDictionary(g => g.Key.ToString(), g => g.Count()),
            TicketsByPriority = tickets
                .GroupBy(t => t.Priority)
                .ToDictionary(g => g.Key.ToString(), g => g.Count())
        };

        return Ok(report);
    }

    /// <summary>
    /// Get customer satisfaction metrics
    /// </summary>
    [HttpGet("satisfaction")]
    public async Task<ActionResult<SatisfactionReport>> GetSatisfactionMetrics()
    {
        var totalCustomers = await _context.Customers.CountAsync();
        var activeCustomers = await _context.Customers
            .Where(c => c.Tickets.Any(t => t.CreatedAt >= DateTime.UtcNow.AddMonths(-3)))
            .CountAsync();

        var report = new SatisfactionReport
        {
            TotalCustomers = totalCustomers,
            ActiveCustomers = activeCustomers,
            CustomerRetentionRate = totalCustomers > 0 ? (double)activeCustomers / totalCustomers * 100 : 0
        };

        return Ok(report);
    }

    /// <summary>
    /// Get agent performance metrics
    /// </summary>
    [HttpGet("agent-performance")]
    public async Task<ActionResult<IEnumerable<AgentPerformance>>> GetAgentPerformance(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        startDate ??= DateTime.UtcNow.AddMonths(-1);
        endDate ??= DateTime.UtcNow;

        var agents = await _context.Users
            .Include(u => u.AssignedTickets.Where(t => t.CreatedAt >= startDate && t.CreatedAt <= endDate))
            .Where(u => u.AssignedTickets.Any())
            .ToListAsync();

        var performance = agents.Select(agent =>
        {
            var assignedTickets = agent.AssignedTickets.ToList();
            var resolvedTickets = assignedTickets
                .Where(t => t.Status == TicketStatus.Resolved || t.Status == TicketStatus.Closed)
                .ToList();

            return new AgentPerformance
            {
                AgentName = agent.FullName,
                TotalAssignedTickets = assignedTickets.Count,
                ResolvedTickets = resolvedTickets.Count,
                AverageResolutionTimeHours = resolvedTickets
                    .Where(t => t.ResolvedAt.HasValue)
                    .Select(t => (t.ResolvedAt!.Value - t.CreatedAt).TotalHours)
                    .DefaultIfEmpty(0)
                    .Average()
            };
        }).ToList();

        return Ok(performance);
    }
}

public class KpiReport
{
    public string Period { get; set; } = string.Empty;
    public int TotalTickets { get; set; }
    public int OpenTickets { get; set; }
    public int InProgressTickets { get; set; }
    public int ResolvedTickets { get; set; }
    public int CriticalTickets { get; set; }
    public double AverageResolutionTimeHours { get; set; }
    public Dictionary<string, int> TicketsByCategory { get; set; } = new();
    public Dictionary<string, int> TicketsByPriority { get; set; } = new();
}

public class SatisfactionReport
{
    public int TotalCustomers { get; set; }
    public int ActiveCustomers { get; set; }
    public double CustomerRetentionRate { get; set; }
}

public class AgentPerformance
{
    public string AgentName { get; set; } = string.Empty;
    public int TotalAssignedTickets { get; set; }
    public int ResolvedTickets { get; set; }
    public double AverageResolutionTimeHours { get; set; }
}
