using System.Net.Http;
using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using SupportSystem.Application.Interfaces;
using SupportSystem.Domain.Entities;

namespace SupportSystem.Application.Services;

// Adaptador que encapsula chamadas HTTP para provedores de IA responsáveis por recomendações.
public class AIService : IAIService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AIService> _logger;

    // Cria o serviço definindo cliente HTTP e logger para rastreabilidade.
    public AIService(HttpClient httpClient, ILogger<AIService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<string?> GerarSugestaoAsync(Ticket ticket, CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrWhiteSpace(ticket.SugestaoIa))
        {
            // Preserva sugestões previamente armazenadas para evitar chamadas externas desnecessárias.
            return ticket.SugestaoIa;
        }

        try
        {
            var payload = new
            {
                ticketId = ticket.Id,
                titulo = ticket.Titulo,
                descricao = ticket.Descricao,
                categoria = ticket.Categoria.ToString(),
                prioridade = ticket.Prioridade.ToString(),
                status = ticket.Status.ToString(),
                solicitante = ticket.Solicitante
            };

            using var response = await _httpClient.PostAsJsonAsync("ai/suggestions", payload, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "IA retornou status inesperado {StatusCode} para ticket {TicketId}",
                    response.StatusCode,
                    ticket.Id);
                return null;
            }

            var suggestion = await response.Content.ReadFromJsonAsync<AiSuggestionResponse>(cancellationToken: cancellationToken);

            if (suggestion is null || string.IsNullOrWhiteSpace(suggestion.Suggestion))
            {
                return null;
            }

            return suggestion.Suggestion;
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            // Trata cenários de indisponibilidade de IA sem impactar o fluxo de atendimento.
            _logger.LogWarning(ex, "Falha ao consultar serviço de IA para o ticket {TicketId}", ticket.Id);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro inesperado ao integrar com IA para o ticket {TicketId}", ticket.Id);
            return null;
        }
    }

    private sealed record AiSuggestionResponse(string? Suggestion);
}
