using SupportSystem.Domain.Enums;

namespace SupportSystem.Application.DTOs;

// Dados flexíveis utilizados para atualizar campos de um ticket.
public record UpdateTicketRequest(
    string? Titulo, // Novo título, quando informado.
    TicketPriority? Prioridade, // Prioridade ajustada.
    TicketStatus? Status, // Status recalculado.
    string? Solicitante, // Solicitante atualizado.
    string? SugestaoIa // Nova sugestão de IA.
);