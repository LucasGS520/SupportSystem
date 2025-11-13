using Microsoft.ML;
using Microsoft.ML.Data;
using SupportSystem.Application.DTOs;
using SupportSystem.Application.Interfaces;
using SupportSystem.Domain.Enums;

namespace SupportSystem.Application.Services;

public class TicketClassificationService : ITicketClassificationService
{
    private readonly MLContext _mlContext;

    public TicketClassificationService()
    {
        _mlContext = new MLContext(seed: 0);
    }

    public Task<TicketClassificationResult> ClassifyTicketAsync(string title, string description)
    {
        // Simple rule-based classification (can be replaced with trained ML model)
        var result = new TicketClassificationResult();
        
        var text = $"{title} {description}".ToLowerInvariant();
        
        // Category classification
        if (text.Contains("payment") || text.Contains("invoice") || text.Contains("bill"))
        {
            result.PredictedCategory = TicketCategory.Billing;
            result.CategoryConfidence = 0.85f;
        }
        else if (text.Contains("bug") || text.Contains("error") || text.Contains("crash"))
        {
            result.PredictedCategory = TicketCategory.Bug;
            result.CategoryConfidence = 0.80f;
        }
        else if (text.Contains("login") || text.Contains("password") || text.Contains("access"))
        {
            result.PredictedCategory = TicketCategory.Account;
            result.CategoryConfidence = 0.75f;
        }
        else if (text.Contains("feature") || text.Contains("request") || text.Contains("enhancement"))
        {
            result.PredictedCategory = TicketCategory.Feature;
            result.CategoryConfidence = 0.70f;
        }
        else
        {
            result.PredictedCategory = TicketCategory.Technical;
            result.CategoryConfidence = 0.60f;
        }
        
        // Priority classification
        if (text.Contains("urgent") || text.Contains("critical") || text.Contains("down") || text.Contains("production"))
        {
            result.PredictedPriority = TicketPriority.Critical;
            result.PriorityConfidence = 0.90f;
        }
        else if (text.Contains("important") || text.Contains("high priority") || text.Contains("asap"))
        {
            result.PredictedPriority = TicketPriority.High;
            result.PriorityConfidence = 0.80f;
        }
        else if (text.Contains("soon") || text.Contains("moderate"))
        {
            result.PredictedPriority = TicketPriority.Medium;
            result.PriorityConfidence = 0.70f;
        }
        else
        {
            result.PredictedPriority = TicketPriority.Low;
            result.PriorityConfidence = 0.60f;
        }
        
        return Task.FromResult(result);
    }
}
