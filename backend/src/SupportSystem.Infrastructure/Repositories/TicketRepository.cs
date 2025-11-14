using Microsoft.EntityFrameworkCore;
using SupportSystem.Domain.Entities;
using SupportSystem.Domain.Repositories;
using SupportSystem.Infrastructure.Persistence;

namespace SupportSystem.Infrastructure.Repositories;

// Implementação baseada em EF Core para persistir tickets.
public class TicketRepository : ITicketRepository
{
    private readonly SupportSystemContext _context;

    // Recebe o contexto configurado via DI.
    public TicketRepository(SupportSystemContext context)
    {
        _context = context;
    }

    // Insere um ticket e confirma a transação.
    public async Task<Ticket> AddAsync(Ticket ticket, CancellationToken cancellationToken = default)
    {
        await _context.Tickets.AddAsync(ticket, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return ticket;
    }

    // Remove um ticket existente e salva a alteração.
    public async Task DeleteAsync(Ticket ticket, CancellationToken cancellationToken = default)
    {
        _context.Tickets.Remove(ticket);
        await _context.SaveChangesAsync(cancellationToken);
    }


    // Retorna todos os tickets ordenados por data de abertura.
    public async Task<IReadOnlyList<Ticket>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Tickets
            .AsNoTracking()
            .OrderByDescending(t => t.AbertoEm)
            .ThenByDescending(t => t.Id)
            .ToListAsync(cancellationToken);
    }

    // Busca um ticket pelo identificador primário.
    public async Task<Ticket?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Tickets.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    // Marca um ticket como modificado e salva as alterações.
    public async Task UpdateAsync(Ticket ticket, CancellationToken cancellationToken = default)
    {
        _context.Tickets.Update(ticket);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
