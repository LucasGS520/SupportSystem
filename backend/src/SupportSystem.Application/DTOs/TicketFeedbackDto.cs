namespace SupportSystem.Application.DTOs;

// Dados estruturados do feedback associado a um ticket.
public record class TicketFeedbackDto
{
    // Nota atribuída pelo usuário, utilizando escala de 1 a 5.
    public int? Nota { get; init; }

    // Comentário livre registrado pelo usuário.
    public string? Comentario { get; init; }

    // Data e hora em UTC de quando o feedback foi registrado.
    public DateTime? RegistradoEm { get; init; }
}
