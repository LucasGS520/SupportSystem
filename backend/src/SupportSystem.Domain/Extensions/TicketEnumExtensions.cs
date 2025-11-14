using SupportSystem.Domain.Enums;

namespace SupportSystem.Domain.Extensions;

// Métodos auxiliares para padronizar textos exibidos dos enums de ticket.
public static class TicketEnumExtensions
{

    // Converte a prioridade em um texto legível para UI.
    public static string ToDisplayName(this TicketPriority priority) =>
        priority switch
        {
            TicketPriority.Baixa => "Baixa",
            TicketPriority.Media => "Média",
            TicketPriority.Alta => "Alta",
            _ => priority.ToString()
        };

    // Converte o status em descrição amigável.
    public static string ToDisplayName(this TicketStatus status) =>
        status switch
        {
            TicketStatus.Aguardando => "Aguardando análise",
            TicketStatus.EmAndamento => "Em andamento",
            TicketStatus.Resolvido => "Resolvido",
            TicketStatus.Critico => "Crítico",
            _ => status.ToString()
        };

    // Converte a categoria do ticket em texto amigável para exibição.
    public static string ToDisplayName(this TicketCategory category) =>
        category switch
        {
            TicketCategory.Redes => "Infraestrutura e redes",
            TicketCategory.SuporteAoUsuario => "Suporte ao usuário",
            TicketCategory.Aplicacoes => "Aplicações",
            TicketCategory.Seguranca => "Segurança da informação",
            TicketCategory.Outros => "Outros",
            _ => category.ToString()
        };
}
