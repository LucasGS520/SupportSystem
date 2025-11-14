using SupportSystem.Domain.Entities;
using SupportSystem.Domain.Enums;

namespace SupportSystem.Application.Interfaces;

// Define o contrato para serviços que disparam notificações sobre eventos relevantes do ciclo de vida de tickets.
public interface INotificationService
{
    // Envia comunicações quando um ticket tem o status alterado, permitindo integrações como e-mail ou push.
    Task NotificarMudancaStatusAsync(
        Ticket ticket,
        TicketStatus statusAnterior,
        CancellationToken cancellationToken = default);
}