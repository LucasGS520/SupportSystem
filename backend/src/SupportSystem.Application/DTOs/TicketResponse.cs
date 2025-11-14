namespace SupportSystem.Application.DTOs;

// Estrutura enviada aos consumidores com os dados do ticket.
public record TicketResponse(
    int Id, // Identificador único no banco.
    string Titulo, // Título exibido ao usuário.
    string Prioridade, // Prioridade em formato amigável.
    string Status, // Status em texto para leitura.
    string? Solicitante, // Nome ou setor solicitante.
    DateTime AbertoEm, // Data e hora de abertura em UTC.
    string? AbertoHa, // Descrição relativa do tempo decorrido.
    string? SugestaoIa // Sugestão opcional de resolução.
);
