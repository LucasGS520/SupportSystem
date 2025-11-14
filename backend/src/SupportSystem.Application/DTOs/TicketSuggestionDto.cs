namespace SupportSystem.Application.DTOs;

// Representa uma sugestão de solução apresentada para um ticket.
public record class TicketSuggestionDto
{
    // Título ou rótulo da sugestão exibido ao usuário.
    public required string Titulo { get; init; }

    // Descrição objetiva da recomendação proposta.
    public required string Descricao { get; init; }

    // Identifica a fonte (IA ou Base de conhecimento) da sugestão.
    public required string Fonte { get; init; }
}