using System;
using System.Coollections.Generic;

namespace SupportSystem.Application.DTOs;

// Estrutura enviada aos consumidores com os dados do ticket.
public record class TicketResponse
{
    // Identificador único no banco.
    public int Id { get; init; }

    // Título exibido ao usuário.
    public required string Titulo { get; init; }

    // Prioridade em formato amigável.
    public required string Prioridade { get; init; }

    // Status em texto para leitura.
    public required string Status { get; init; }

    // Identificador do usuário que abriu o chamado.
    public int OwnerId { get; init; }

    // Identificador do técnico designado, quando houver.
    public int? AssignedTechnicianId { get; init; }

    // Categoria do ticket em formato amigável para a interface.
    public required string Categoria { get; init; }

    // Data e hora alvo do SLA, quando definida.
    public DateTime? SlaTarget { get; init; }

    // Nome ou setor solicitante.
    public string? Solicitante { get; init; }

    // Data e hora de abertura em UTC.
    public DateTime AbertoEm { get; init; }

    // Descrição relativa do tempo decorrido.
    public string? AbertoHa { get; init; }

    // Sugestão opcional de resolução.
    public string? SugestaoIa { get; init; }

    // Feedback associado ao ticket.
    public TicketFeedbackDto? Feedback { get; init; }

    // Lista de sugestões contextuais vindas da IA e da base de conhecimento.
    public IReadOnlyList<TicketSuggestionDto> Suggestions { get; init; } = Array.Empty<TicketSuggestionDto>();

    // Indica se os dados apresentados contam com consentimento válido.
    public bool ConsentimentoDados { get; init; }
}
