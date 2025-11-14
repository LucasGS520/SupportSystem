using SupportSystem.Application.DTOs;

namespace SupportSystem.Application.Interfaces;

// Define operações de orquestração sobre o ciclo de vida de tickets.
public interface ITicketService
{
    // Retorna todos os tickets visíveis para o portal.
    Task<IReadOnlyList<TicketResponse>> GetAllAsync(CancellationToken cancellationToken = default);

    // Busca os dados de um ticket específico.
    Task<TicketResponse?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    // Cria um ticket e devolve o modelo de resposta padrão.
    Task<TicketResponse> CreateAsync(CreateTicketRequest request, CancellationToken cancellationToken = default);

    // Atualiza campos aceitos do ticket informado.
    Task<bool> UpdateAsync(int id, UpdateTicketRequest request, CancellationToken cancellationToken = default);


    // Remove o ticket indicado do repositório.
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
