namespace SupportSystem.Domain.Entities;

using SupportSystem.Domain.Enums;

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

    // Pessoa ou setor que solicitou o suporte.
    public string? Solicitante { get; set; }

    // Data/hora de abertura registrada em UTC.
    public DateTime AbertoEm { get; set; } = DateTime.UtcNow;

    // Sugestão de resolução gerada pela IA, quando existir.
    public string? SugestaoIa { get; set; }
}
