namespace SupportSystem.Domain.Enums;

// Níveis de prioridade aplicados aos tickets.
public enum TicketPriority : byte
{

    // Incidentes que podem aguardar atendimento.
    Baixa = 1,

    // Situações padrão com urgência moderada.
    Media = 2,

    // Chamados críticos que exigem resposta imediata.
    Alta = 3
}
