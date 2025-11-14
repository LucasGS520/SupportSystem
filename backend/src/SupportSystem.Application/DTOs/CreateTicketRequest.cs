using SupportSystem.Domain.Enums;

namespace SupportSystem.Application.DTOs;

// Representa os dados necessários para registrar um novo ticket.

public record CreateTicketRequest(
    string Titulo, // Título obrigatório exibido na lista.
    TicketPriority Prioridade, // Prioridade inicial do ticket.
    TicketStatus Status, // Status atual informado pelo usuário.
    string? Solicitante, // Nome ou setor que abriu o ticket.
    string? SugestaoIa
); // Sugestão opcional gerada pela IA.
