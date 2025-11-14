using System;
using System.Collections.Generic;
using System.Linq;
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
    private readonly IKnowledgeBaseRepository _knowledgeBaseRepository;
    private readonly IAIService _aiService;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly INotificationService _notificationService;
    private readonly ILogger<TicketService> _logger;

    // Cria o serviço recebendo repositório, provedor de data e logger.
    public TicketService(
        ITicketRepository ticketRepository,
        IKnowledgeBaseRepository knowledgeBaseRepository,
        IAIService aiService,
        IDateTimeProvider dateTimeProvider,
        INotificationService notificationService,
        ILogger<TicketService> logger)
    {
        _ticketRepository = ticketRepository;
        _knowledgeBaseRepository = knowledgeBaseRepository;
        _aiService = aiService;
        _dateTimeProvider = dateTimeProvider;
        _notificationService = notificationService;
        _logger = logger;
    }

    // Retorna todos os tickets persistidos.
    public async Task<IReadOnlyList<TicketResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var tickets = await _ticketRepository.GetAllAsync(cancellationToken);
        var projections = await Task.WhenAll(
            tickets.Select(ticket => MapToResponseWithSuggestionsAsync(ticket, cancellationToken)));

        return projections.ToList();
    }

    // Consulta um ticket específico pelo identificador.
    public async Task<TicketResponse?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var ticket = await _ticketRepository.GetByIdAsync(id, cancellationToken);
        return ticket is null
            ? null
            : await MapToResponseWithSuggestionsAsync(ticket, cancellationToken);
    }

    // Cria um ticket novo e devolve a projeção pronta para API.
    public async Task<TicketResponse> CreateAsync(CreateTicketRequest request, CancellationToken cancellationToken = default)
    {
        if (!request.ConsentimentoDados)
        {
            // Rejeita a abertura porque a LGPD exige consentimento explícito antes da coleta.
            throw new InvalidOperationException("Consentimento obrigatório para registrar o chamado.");
        }

        if (request.OwnerId <= 0)
        {
            throw new InvalidOperationException("Usuário autenticado não identificado para criação do chamado.");
        }

        var consentTime = _dateTimeProvider.UtcNow;

        var ticket = new Ticket
        {
            Titulo = request.Titulo,
            Prioridade = request.Prioridade,
            Status = request.Status,
            OwnerId = request.OwnerId,
            AssignedTechnicianId = request.AssignedTechnicianId,
            Categoria = request.Categoria,
            Descricao = request.Descricao,
            SlaTarget = request.SlaTarget,
            Solicitante = request.Solicitante,
            AbertoEm = consentTime,
            SugestaoIa = request.SugestaoIa,
            Feedback = MapToEntity(request.Feedback),
            ConsentimentoDados = request.ConsentimentoDados,
            ConsentimentoRegistradoEm = consentTime
        };

        var created = await _ticketRepository.AddAsync(ticket, cancellationToken);
        _logger.LogInformation("Ticket {TicketId} criado com sucesso", created.Id);

        return await MapToResponseWithSuggestionsAsync(created, cancellationToken);
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

        var statusAnterior = ticket.Status;

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
        if (request.Descricao is not null)
        {
            ticket.Descricao = request.Descricao;
        }
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

        if (request.ConsentimentoDados.HasValue)
        {
            ticket.ConsentimentoDados = request.ConsentimentoDados.Value;
            ticket.ConsentimentoRegistradoEm = _dateTimeProvider.UtcNow;

            if (!ticket.ConsentimentoDados)
            {
                // Assim que o consentimento é revogado, limpamos campos pessoais sensíveis.
                ticket.Solicitante = null;
                ticket.Descricao = null;

                if (ticket.Feedback is not null)
                {
                    ticket.Feedback.Comentario = null;
                }
            }
        }

        await _ticketRepository.UpdateAsync(ticket, cancellationToken);
        _logger.LogInformation("Ticket {TicketId} atualizado", ticket.Id);

        var statusAlterado = request.Status.HasValue && request.Status.Value != statusAnterior;
        if (statusAlterado)
        {
            // Garante o disparo imediato da notificação após persistir a mudança de status.
            await _notificationService.NotificarMudancaStatusAsync(ticket, statusAnterior, cancellationToken);
        }

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

    // Converte a entidade em DTO e agrega sugestões oriundas da IA e da base de conhecimento.
    private async Task<TicketResponse> MapToResponseWithSuggestionsAsync(
        Ticket ticket,
        CancellationToken cancellationToken)
    {
        var response = MapToResponse(ticket);
        var suggestions = await BuildSuggestionsAsync(ticket, cancellationToken);

        var sugestaoPrimaria = response.SugestaoIa;
        if (string.IsNullOrWhiteSpace(sugestaoPrimaria))
        {
            // Define uma sugestão principal para compatibilidade com a UI legada.
            sugestaoPrimaria = suggestions.FirstOrDefault(s => s.Fonte == "Assistente virtual")?.Descricao
                ?? suggestions.FirstOrDefault()?.Descricao
                ?? response.SugestaoIa;
        }

        return response with
        {
            SugestaoIa = sugestaoPrimaria,
            Suggestions = suggestions
        };
    }

    // Monta a lista de sugestões que será exibida para o ticket atual.
    private async Task<IReadOnlyList<TicketSuggestionDto>> BuildSuggestionsAsync(
        Ticket ticket,
        CancellationToken cancellationToken)
    {
        var suggestions = new List<TicketSuggestionDto>();

        var knowledgeArticles = await _knowledgeBaseRepository.BuscarRelevantesAsync(
            string.Join(' ', new[] { ticket.Titulo, ticket.Descricao }.Where(value => !string.IsNullOrWhiteSpace(value))),
            ticket.Categoria,
            5,
            cancellationToken);

        foreach (var article in knowledgeArticles)
        {
            suggestions.Add(new TicketSuggestionDto
            {
                Titulo = article.Titulo,
                Descricao = BuildResumo(article.Conteudo),
                Fonte = "Base de conhecimento"
            });
        }

        var aiSuggestion = await _aiService.GerarSugestaoAsync(ticket, cancellationToken);
        if (!string.IsNullOrWhiteSpace(aiSuggestion))
        {
            // Insere a resposta da IA no topo para destacar recomendações dinâmicas.
            suggestions.Insert(0, new TicketSuggestionDto
            {
                Titulo = "Sugestão da IA",
                Descricao = aiSuggestion!,
                Fonte = "Assistente virtual"
            });
        }

        return suggestions;
    }

    // Garante que o conteúdo exibido na interface seja curto e de leitura rápida.
    private static string BuildResumo(string conteudo)
    {
        if (string.IsNullOrWhiteSpace(conteudo))
        {
            return string.Empty;
        }

        const int limite = 320;
        return conteudo.Length <= limite
            ? conteudo
            : string.Concat(conteudo.AsSpan(0, limite).TrimEnd(), "...");
    }

    // Converte a entidade em DTO pronto para uso na camada de apresentação.
    private TicketResponse MapToResponse(Ticket ticket)
    {
        var hasConsent = ticket.ConsentimentoDados;

        var safeSolicitante = hasConsent ? ticket.Solicitante : null;
        var feedback = PrepareFeedbackForResponse(ticket.Feedback, hasConsent);

        return new TicketResponse
        {
            Id = ticket.Id,
            Titulo = ticket.Titulo,
            Prioridade = ticket.Prioridade.ToDisplayName(),
            Status = ticket.Status.ToDisplayName(),
            OwnerId = ticket.OwnerId,
            AssignedTechnicianId = ticket.AssignedTechnicianId,
            Categoria = ticket.Categoria.ToDisplayName(),
            Descricao = hasConsent ? ticket.Descricao : null,
            SlaTarget = ticket.SlaTarget,
            Solicitante = safeSolicitante,
            AbertoEm = ticket.AbertoEm,
            AbertoHa = FormatRelativeTime(ticket.AbertoEm),
            SugestaoIa = ticket.SugestaoIa,
            Feedback = feedback,
            ConsentimentoDados = hasConsent
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

    // Ajusta o feedback levando em conta o consentimento do usuário.
    private static TicketFeedbackDto? PrepareFeedbackForResponse(TicketFeedback? feedback, bool includeSensitive)
    {
        if (feedback is null)
        {
            return null;
        }

        var copy = new TicketFeedback
        {
            Nota = feedback.Nota,
            Comentario = includeSensitive ? feedback.Comentario : null,
            RegistradoEm = feedback.RegistradoEm
        };

        return MapToDto(copy);
    }
}
