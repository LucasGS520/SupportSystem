namespace SupportSystem.Domain.Enums;

// Possíveis estágios do fluxo de tratamento do ticket.
public enum TicketStatus : byte
{
    // Ticket registrado, aguardando triagem.
    Aguardando = 1,

    // Ticket em tratamento ativo pela equipe.
    EmAndamento = 2,

    // Ticket finalizado e validado.
    Resolvido = 3,

    // Ticket com impacto crítico ou indisponibilidade.
    Critico = 4
}
