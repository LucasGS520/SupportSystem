using Microsoft.Extensions.Logging;
using SupportSystem.Application.Abstractions;
using SupportSystem.Application.DTOs;
using SupportSystem.Application.Interfaces;
using SupportSystem.Domain.Entities;
using SupportSystem.Domain.Extensions;
using SupportSystem.Domain.Repositories;

namespace SupportSystem.Application.Services;

// Implementa as regras de orquestração para o ciclo de vida de tickets.
public class TicketService : ITicketService
{
    private readonly ITicketRepository _ticketRepository;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ILogger<TicketService> _logger;

    // Cria o serviço recebendo repositório, provedor de data e logger.
    public TicketService(
        ITicketRepository ticketRepository,
        IDateTimeProvider dateTimeProvider,
        ILogger<TicketService> logger)
    {
        _ticketRepository = ticketRepository;
        _dateTimeProvider = dateTimeProvider;
        _logger = logger;
    }

    // Retorna todos os tickets persistidos.
    public async Task<IReadOnlyList<TicketResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var tickets = await _ticketRepository.GetAllAsync(cancellationToken);
        return tickets.Select(MapToResponse).ToList();
    }

    // Consulta um ticket específico pelo identificador.
    public async Task<TicketResponse?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var ticket = await _ticketRepository.GetByIdAsync(id, cancellationToken);
        return ticket is null ? null : MapToResponse(ticket);
    }

    // Cria um ticket novo e devolve a projeção pronta para API.
    public async Task<TicketResponse> CreateAsync(CreateTicketRequest request, CancellationToken cancellationToken = default)
    {
        var ticket = new Ticket
        {
            Titulo = request.Titulo,
            Prioridade = request.Prioridade,
            Status = request.Status,
            OwnerId = request.OwnerId,
            AssignedTechnicianId = request.AssignedTechnicianId,
            Categoria = request.Categoria,
            SlaTarget = request.SlaTarget,
            Solicitante = request.Solicitante,
            AbertoEm = _dateTimeProvider.UtcNow,
            SugestaoIa = request.SugestaoIa,
            Feedback = MapToEntity(request.Feedback)
        };

        var created = await _ticketRepository.AddAsync(ticket, cancellationToken);
        _logger.LogInformation("Ticket {TicketId} criado com sucesso", created.Id);

        return MapToResponse(created);
    }

    // Atualiza campos de um ticket existente.
    public async Task<bool> UpdateAsync(int id, UpdateTicketRequest request, CancellationToken cancellationToken = default)
    {
        var ticket = await _ticketRepository.GetByIdAsync(id, cancellationToken);
        if (ticket is null)
        {
            _logger.LogWarning("Tentativa de atualizar ticket inexistente {TicketId}", id);
            return false;
        }

        ticket.Titulo = request.Titulo ?? ticket.Titulo;
        ticket.Prioridade = request.Prioridade ?? ticket.Prioridade;
        ticket.Status = request.Status ?? ticket.Status;
        ticket.Solicitante = request.Solicitante ?? ticket.Solicitante;
        ticket.SugestaoIa = request.SugestaoIa ?? ticket.SugestaoIa;

        if (request.OwnerId.HasValue)
        {
            ticket.OwnerId = request.OwnerId.Value;
        }

        ticket.AssignedTechnicianId = request.AssignedTechnicianId ?? ticket.AssignedTechnicianId;
        ticket.Categoria = request.Categoria ?? ticket.Categoria;
        ticket.SlaTarget = request.SlaTarget ?? ticket.SlaTarget;

        if (request.Feedback is not null)
        {
            if (ticket.Feedback is null)
            {
                ticket.Feedback = MapToEntity(request.Feedback);
            }
            else
            {
                ticket.Feedback.Nota = request.Feedback.Nota;
                ticket.Feedback.Comentario = request.Feedback.Comentario;
                ticket.Feedback.RegistradoEm = request.Feedback.RegistradoEm;
            }
        }

        await _ticketRepository.UpdateAsync(ticket, cancellationToken);
        _logger.LogInformation("Ticket {TicketId} atualizado", ticket.Id);

        return true;
    }

    // Remove definitivamente um ticket existente.
    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var ticket = await _ticketRepository.GetByIdAsync(id, cancellationToken);
        if (ticket is null)
        {
            _logger.LogWarning("Tentativa de remover ticket inexistente {TicketId}", id);
            return false;
        }

        await _ticketRepository.DeleteAsync(ticket, cancellationToken);
        _logger.LogInformation("Ticket {TicketId} removido", ticket.Id);

        return true;
    }

    // Converte a entidade em DTO pronto para uso na camada de apresentação.
    private TicketResponse MapToResponse(Ticket ticket)
    {
        return new TicketResponse
        {
            Id = ticket.Id,
            Titulo = ticket.Titulo,
            Prioridade = ticket.Prioridade.ToDisplayName(),
            Status = ticket.Status.ToDisplayName(),
            OwnerId = ticket.OwnerId,
            AssignedTechnicianId = ticket.AssignedTechnicianId,
            Categoria = ticket.Categoria.ToDisplayName(),
            SlaTarget = ticket.SlaTarget,
            Solicitante = ticket.Solicitante,
            AbertoEm = ticket.AbertoEm,
            AbertoHa = FormatRelativeTime(ticket.AbertoEm),
            SugestaoIa = ticket.SugestaoIa,
            Feedback = MapToDto(ticket.Feedback)
        };
    }

    // Formata o tempo decorrido desde a abertura do ticket.
    private string? FormatRelativeTime(DateTime openedAtUtc)
    {
        if (openedAtUtc == default)
        {
            return null;
        }

        var now = _dateTimeProvider.UtcNow;
        var diff = now - DateTime.SpecifyKind(openedAtUtc, DateTimeKind.Utc);

        if (diff.TotalSeconds < 60)
        {
            return "agora mesmo";
        }

        if (diff.TotalMinutes < 60)
        {
            var minutes = Math.Max(1, (int)Math.Round(diff.TotalMinutes));
            return $"há {minutes} min";
        }

        if (diff.TotalHours < 24)
        {
            var hours = Math.Max(1, (int)Math.Round(diff.TotalHours));
            return $"há {hours}h";
        }

        if (diff.TotalDays < 7)
        {
            var days = Math.Max(1, (int)Math.Round(diff.TotalDays));
            return days == 1 ? "há 1 dia" : $"há {days} dias";
        }

        var localTime = DateTime.SpecifyKind(openedAtUtc, DateTimeKind.Utc).ToLocalTime();
        return localTime.ToString("dd/MM/yyyy HH:mm");
    }

    // Converte o DTO de feedback em entidade agregada.
    private static TicketFeedback? MapToEntity(TicketFeedbackDto? feedback)
    {
        if (feedback is null)
        {
            return null;
        }

        return new TicketFeedback
        {
            Nota = feedback.Nota,
            Comentario = feedback.Comentario,
            RegistradoEm = feedback.RegistradoEm
        };
    }

    // Converte a entidade de feedback em DTO para transporte.
    private static TicketFeedbackDto? MapToDto(TicketFeedback? feedback)
    {
        if (feedback is null)
        {
            return null;
        }

        return new TicketFeedbackDto
        {
            Nota = feedback.Nota,
            Comentario = feedback.Comentario,
            RegistradoEm = feedback.RegistradoEm
        };
    }
}
