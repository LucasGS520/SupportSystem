using SupportSystem.Domain.Enums;

namespace SupportSystem.Application.DTOs;

// Representa os dados necessários para registrar um novo ticket.
public record class CreateTicketRequest
{
    // Título obrigatório exibido na lista.
    public required string Titulo { get; init; }

    // Prioridade inicial do ticket.
    public TicketPriority Prioridade { get; init; } = TicketPriority.Media;

    // Status atual informado pelo usuário.
    public TicketStatus Status { get; init; } = TicketStatus.Aguardando;

    // Identificador do usuário autenticado que abriu o chamado.
    public required int OwnerId { get; init; }

    // Identificador do técnico designado, quando já definido.
    public int? AssignedTechnicianId { get; init; }

    // Categoria atribuída para fins de roteamento e métricas.
    public TicketCategory Categoria { get; init; } = TicketCategory.Outros;

    // Data e hora alvo para cumprimento do SLA.
    public DateTime? SlaTarget { get; init; }

    // Nome ou setor que abriu o ticket.
    public string? Solicitante { get; init; }

    // Sugestão opcional gerada pela IA.
    public string? SugestaoIa { get; init; }

    // Feedback inicial informado pelo usuário, quando capturado junto à abertura.
    public TicketFeedbackDto? Feedback { get; init; }
}
