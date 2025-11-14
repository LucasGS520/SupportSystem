using System.Linq;
using Microsoft.EntityFrameworkCore;
using SupportSystem.Application.Abstractions;
using SupportSystem.Domain.Entities;
using SupportSystem.Domain.Enums;
using SupportSystem.Domain.Repositories;
using SupportSystem.Infrastructure.Persistence;

namespace SupportSystem.Infrastructure.Repositories;

// Implementação baseada em EF Core para persistir tickets.
public class TicketRepository : ITicketRepository
{
    private readonly SupportSystemContext _context;
    private readonly ISensitiveDataProtector _protector;

    // Recebe o contexto configurado via DI.
    public TicketRepository(SupportSystemContext context, ISensitiveDataProtector protector)
    {
        _context = context;
        _protector = protector;
    }

    // Insere um ticket e confirma a transação.
    public async Task<Ticket> AddAsync(Ticket ticket, CancellationToken cancellationToken = default)
    {
        var entity = MapToStorage(ticket);

        await _context.Tickets.AddAsync(entity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return MapToDomain(entity);
    }

    // Remove um ticket existente e salva a alteração.
    public async Task DeleteAsync(Ticket ticket, CancellationToken cancellationToken = default)
    {
        var entity = new Ticket { Id = ticket.Id };
        _context.Tickets.Attach(entity);
        _context.Tickets.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }


    // Retorna todos os tickets ordenados por data de abertura.
    public async Task<IReadOnlyList<Ticket>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var entities = await _context.Tickets
            .AsNoTracking()
            .OrderByDescending(t => t.AbertoEm)
            .ThenByDescending(t => t.Id)
            .ToListAsync(cancellationToken);

        return entities.Select(MapToDomain).ToList();
    }

    // Busca um ticket pelo identificador primário.
    public async Task<Ticket?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Tickets
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

        return entity is null ? null : MapToDomain(entity);
    }

    // Marca um ticket como modificado e salva as alterações.
    public async Task UpdateAsync(Ticket ticket, CancellationToken cancellationToken = default)
    {
        var entity = MapToStorage(ticket);
        _context.Tickets.Update(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    // Obtém os tickets associados a um proprietário específico.
    public async Task<IReadOnlyList<Ticket>> GetByOwnerAsync(int ownerId, CancellationToken cancellationToken = default)
    {
        var entities = await _context.Tickets
            .AsNoTracking()
            .Where(t => t.OwnerId == ownerId)
            .OrderByDescending(t => t.AbertoEm)
            .ToListAsync(cancellationToken);

        return entities.Select(MapToDomain).ToList();
    }

    // Exclui tickets com data de abertura anterior ao limite configurado.
    public async Task<int> DeleteOlderThanAsync(DateTime cutoffUtc, CancellationToken cancellationToken = default)
    {
        return await _context.Tickets
            .Where(t => t.AbertoEm < cutoffUtc && t.Status == TicketStatus.Resolvido)
            .ExecuteDeleteAsync(cancellationToken);
    }

    // Remove tickets associados ao proprietário informado.
    public async Task<int> DeleteByOwnerAsync(int ownerId, CancellationToken cancellationToken = default)
    {
        return await _context.Tickets
            .Where(t => t.OwnerId == ownerId)
            .ExecuteDeleteAsync(cancellationToken);
    }

    // Cria uma cópia protegida do ticket para ser persistida.
    private Ticket MapToStorage(Ticket ticket)
    {
        var solicitanteSeguro = ticket.ConsentimentoDados ? ticket.Solicitante : null;
        var feedbackSeguro = ticket.ConsentimentoDados ? ticket.Feedback : SanitizeFeedback(ticket.Feedback);

        return new Ticket
        {
            Id = ticket.Id,
            Titulo = ticket.Titulo,
            Prioridade = ticket.Prioridade,
            Status = ticket.Status,
            OwnerId = ticket.OwnerId,
            AssignedTechnicianId = ticket.AssignedTechnicianId,
            Categoria = ticket.Categoria,
            SlaTarget = ticket.SlaTarget,
            Solicitante = Protect(solicitanteSeguro),
            AbertoEm = ticket.AbertoEm,
            Feedback = MapFeedbackToStorage(feedbackSeguro),
            SugestaoIa = ticket.SugestaoIa,
            ConsentimentoDados = ticket.ConsentimentoDados,
            ConsentimentoRegistradoEm = ticket.ConsentimentoRegistradoEm
        };
    }

    // Converte o registro persistido em uma instância de domínio com dados legíveis.
    private Ticket MapToDomain(Ticket entity)
    {
        var ticket = new Ticket
        {
            Id = entity.Id,
            Titulo = entity.Titulo,
            Prioridade = entity.Prioridade,
            Status = entity.Status,
            OwnerId = entity.OwnerId,
            AssignedTechnicianId = entity.AssignedTechnicianId,
            Categoria = entity.Categoria,
            SlaTarget = entity.SlaTarget,
            Solicitante = Unprotect(entity.Solicitante),
            AbertoEm = entity.AbertoEm,
            Feedback = MapFeedbackToDomain(entity.Feedback),
            SugestaoIa = entity.SugestaoIa,
            ConsentimentoDados = entity.ConsentimentoDados,
            ConsentimentoRegistradoEm = entity.ConsentimentoRegistradoEm
        };

        if (!ticket.ConsentimentoDados)
        {
            ticket.Solicitante = null;
            if (ticket.Feedback is not null)
            {
                ticket.Feedback.Comentario = null;
            }
        }

        return ticket;
    }

    // Prepara o feedback para armazenamento com proteção de dados sensíveis.
    private TicketFeedback? MapFeedbackToStorage(TicketFeedback? feedback)
    {
        if (feedback is null)
        {
            return null;
        }

        return new TicketFeedback
        {
            Nota = feedback.Nota,
            Comentario = Protect(feedback.Comentario),
            RegistradoEm = feedback.RegistradoEm
        };
    }

    // Reconstrói o feedback descriptografado para uso no domínio.
    private TicketFeedback? MapFeedbackToDomain(TicketFeedback? feedback)
    {
        if (feedback is null)
        {
            return null;
        }

        return new TicketFeedback
        {
            Nota = feedback.Nota,
            Comentario = Unprotect(feedback.Comentario),
            RegistradoEm = feedback.RegistradoEm
        };
    }

    // Remove comentários sensíveis quando o consentimento não existir.
    private static TicketFeedback? SanitizeFeedback(TicketFeedback? feedback)
    {
        if (feedback is null)
        {
            return null;
        }

        return new TicketFeedback
        {
            Nota = feedback.Nota,
            Comentario = null,
            RegistradoEm = feedback.RegistradoEm
        };
    }

    // Protege o texto informado utilizando o provedor de dados sensíveis.
    private string? Protect(string? value) => string.IsNullOrWhiteSpace(value) ? value : _protector.Protect(value);

    // Remove a proteção do texto recebido, tratando chaves inválidas.
    private string? Unprotect(string? value) => string.IsNullOrWhiteSpace(value) ? value : _protector.Unprotect(value);
}
