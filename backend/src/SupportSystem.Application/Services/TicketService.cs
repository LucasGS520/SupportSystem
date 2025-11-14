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
            Solicitante = request.Solicitante,
            AbertoEm = _dateTimeProvider.UtcNow,
            SugestaoIa = request.SugestaoIa
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
        return new TicketResponse(
            ticket.Id,
            ticket.Titulo,
            ticket.Prioridade.ToDisplayName(),
            ticket.Status.ToDisplayName(),
            ticket.Solicitante,
            ticket.AbertoEm,
            FormatRelativeTime(ticket.AbertoEm),
            ticket.SugestaoIa);
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
}
