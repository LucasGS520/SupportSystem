using SupportSystem.Domain.Enums;

namespace SupportSystem.Application.DTOs;

// Dados flexíveis utilizados para atualizar campos de um ticket.
public record class UpdateTicketRequest
{
    // Novo título, quando informado.
    public string? Titulo { get; init; }

    // Prioridade ajustada.
    public TicketPriority? Prioridade { get; init; }

    // Status recalculado.
    public TicketStatus? Status { get; init; }

    // Identificador atualizado do solicitante autenticado.
    public int? OwnerId { get; init; }

    // Identificador do técnico responsável pelo atendimento.
    public int? AssignedTechnicianId { get; init; }

    // Categoria recalculada para melhorar relatórios.
    public TicketCategory? Categoria { get; init; }

    // Data e hora alvo do SLA após renegociação.
    public DateTime? SlaTarget { get; init; }

    // Solicitante atualizado.
    public string? Solicitante { get; init; }

    // Nova sugestão de IA.
    public string? SugestaoIa { get; init; }

    // Feedback atualizado pelo usuário após conclusão do atendimento.
    public TicketFeedbackDto? Feedback { get; init; }
}