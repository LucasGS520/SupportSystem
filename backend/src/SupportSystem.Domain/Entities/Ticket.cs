using SupportSystem.Domain.Enums;

namespace SupportSystem.Domain.Entities;

// Representa o agregado principal de chamados dentro do domínio.
public class Ticket
{
    // Identificador único do ticket na base de dados.
    public int Id { get; set; }

    // Título resumido que descreve o problema.
    public string Titulo { get; set; } = string.Empty;

    // Prioridade definida para o atendimento do ticket.
    public TicketPriority Prioridade { get; set; } = TicketPriority.Media;

    // Status atual do fluxo de atendimento.
    public TicketStatus Status { get; set; } = TicketStatus.Aguardando;

    // Identificador do usuário que abriu o ticket.
    public int OwnerId { get; set; }

    // Identificador do técnico responsável pela tratativa, quando houver.
    public int? AssignedTechnicianId { get; set; }

    // Categoria macro utilizada para roteamento e relatórios.
    public TicketCategory Categoria { get; set; } = TicketCategory.Outros;

    // Data/hora limite em UTC para cumprimento do SLA definido.
    public DateTime? SlaTarget { get; set; }

    // Pessoa ou setor que solicitou o suporte.
    public string? Solicitante { get; set; }

    // Data/hora de abertura registrada em UTC.
    public DateTime AbertoEm { get; set; } = DateTime.UtcNow;

    // Feedback registrado pelo solicitante após a resolução.
    public TicketFeedback? Feedback { get; set; }

    // Sugestão de resolução gerada pela IA, quando existir.
    public string? SugestaoIa { get; set; }
}
