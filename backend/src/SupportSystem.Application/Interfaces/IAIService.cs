using SupportSystem.Domain.Entities;

namespace SupportSystem.Application.Interfaces;

// Define o contrato para integração com provedores de IA responsáveis por sugerir soluções.
public interface IAIService
{
    // Gera uma sugestão textual personalizada para o ticket informado.
    Task<string?> GerarSugestaoAsync(Ticket ticket, CancellationToken cancellationToken = default);
}
