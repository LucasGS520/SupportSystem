using Microsoft.Extensions.Logging;
using SupportSystem.Application.Interfaces;
using SupportSystem.Domain.Entities;
using SupportSystem.Domain.Enums;

namespace SupportSystem.Infrastructure.Notifications;

// Implementa um serviço de notificações via e-mail para eventos gerados pelos tickets.
public class EmailNotificationService : INotificationService
{
    private readonly ILogger<EmailNotificationService> _logger;

    // Inicializa o serviço com um logger para rastrear tentativas de disparo.
    public EmailNotificationService(ILogger<EmailNotificationService> logger)
    {
        _logger = logger;
    }

    // Registra o disparo de uma notificação de alteração de status, simulando a entrega por e-mail.
    public Task NotificarMudancaStatusAsync(
        Ticket ticket,
        TicketStatus statusAnterior,
        CancellationToken cancellationToken = default)
    {
        // Como a integração real ainda não está definida, mantemos o log para acompanhar as mudanças críticas.
        _logger.LogInformation(
            "Ticket {TicketId} mudou do status {StatusAnterior} para {StatusAtual}. Notificação de e-mail enviada.",
            ticket.Id,
            statusAnterior,
            ticket.Status);

        return Task.CompletedTask;
    }
}
