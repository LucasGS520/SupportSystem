namespace SupportSystem.Domain.Entities;

// Representa o feedback fornecido pelo solicitante após o atendimento do ticket.
public class TicketFeedback
{
    // Nota atribuída ao atendimento, considerando uma escala de 1 a 5.
    public int? Nota { get; set; }

    // Comentário livre do usuário detalhando sua percepção do atendimento.
    public string? Comentario { get; set; }

    // Momento em que o feedback foi submetido em UTC para auditoria.
    public DateTime? RegistradoEm { get; set; }
}
