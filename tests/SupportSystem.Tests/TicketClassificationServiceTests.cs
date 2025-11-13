using SupportSystem.Application.Services;
using SupportSystem.Domain.Enums;
using Xunit;

namespace SupportSystem.Tests;

public class TicketClassificationServiceTests
{
    private readonly TicketClassificationService _service;

    public TicketClassificationServiceTests()
    {
        _service = new TicketClassificationService();
    }

    [Fact]
    public async Task ClassifyTicket_BillingKeywords_ReturnsBillingCategory()
    {
        // Arrange
        var title = "Invoice issue";
        var description = "I have a problem with my payment";

        // Act
        var result = await _service.ClassifyTicketAsync(title, description);

        // Assert
        Assert.Equal(TicketCategory.Billing, result.PredictedCategory);
    }

    [Fact]
    public async Task ClassifyTicket_BugKeywords_ReturnsBugCategory()
    {
        // Arrange
        var title = "Application crash";
        var description = "The app crashes when I click on the button";

        // Act
        var result = await _service.ClassifyTicketAsync(title, description);

        // Assert
        Assert.Equal(TicketCategory.Bug, result.PredictedCategory);
    }

    [Fact]
    public async Task ClassifyTicket_UrgentKeywords_ReturnsCriticalPriority()
    {
        // Arrange
        var title = "Urgent: System down";
        var description = "Production environment is completely down";

        // Act
        var result = await _service.ClassifyTicketAsync(title, description);

        // Assert
        Assert.Equal(TicketPriority.Critical, result.PredictedPriority);
    }

    [Fact]
    public async Task ClassifyTicket_AccountKeywords_ReturnsAccountCategory()
    {
        // Arrange
        var title = "Cannot login";
        var description = "I forgot my password and cannot access my account";

        // Act
        var result = await _service.ClassifyTicketAsync(title, description);

        // Assert
        Assert.Equal(TicketCategory.Account, result.PredictedCategory);
    }
}
