using SupportSystem.Domain.Entities;

namespace SupportSystem.Domain.Repositories;

// Contrato base para persistência da entidade Ticket.
public interface ITicketRepository
{
    // Recupera todos os tickets armazenados.
    Task<IReadOnlyList<Ticket>> GetAllAsync(CancellationToken cancellationToken = default);


    // Busca um ticket pelo identificador.
    Task<Ticket?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    // Insere um novo ticket no repositório.
    Task<Ticket> AddAsync(Ticket ticket, CancellationToken cancellationToken = default);

    // Persiste alterações de um ticket existente.
    Task UpdateAsync(Ticket ticket, CancellationToken cancellationToken = default);

    // Remove um ticket da fonte de dados.
    Task DeleteAsync(Ticket ticket, CancellationToken cancellationToken = default);
}
