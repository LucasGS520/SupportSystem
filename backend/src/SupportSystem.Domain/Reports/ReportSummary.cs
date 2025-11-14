using System;

namespace SupportSystem.Domain.Reports;

// Consolida métricas essenciais de atendimento para relatórios e KPIs.
public sealed class ReportSummary
{
    // Quantidade total de tickets considerados no período consultado.
    public int TotalTickets { get; }

    // Total de tickets em status aguardando.
    public int TicketsAguardando { get; }

    // Total de tickets em andamento.
    public int TicketsEmAndamento { get; }

    // Total de tickets resolvidos.
    public int TicketsResolvidos { get; }

    // Total de tickets classificados como críticos.
    public int TicketsCriticos { get; }

    // Quantidade de tickets com feedback preenchido.
    public int TicketsComFeedback { get; }

    // Média aritmética das notas de feedback.
    public double? MediaFeedback { get; }

    // Total de tickets com sugestões geradas pela IA.
    public int TicketsComSugestaoIa { get; }

    // Total de tickets com SLA já vencido e ainda não resolvidos.
    public int TicketsComSlaVencido { get; }

    // Agrupamento de contagens por categoria de ticket.
    public IReadOnlyCollection<ReportCategoryCount> Categorias { get; }

    // Calcula a quantidade de tickets sem feedback a partir do total.
    public int TicketsSemFeedback => Math.Max(0, TotalTickets - TicketsComFeedback);

    // Instancia o resumo consolidando métricas de atendimento e distribuição por categoria.
    public ReportSummary(
        int totalTickets,
        int ticketsAguardando,
        int ticketsEmAndamento,
        int ticketsResolvidos,
        int ticketsCriticos,
        int ticketsComFeedback,
        double? mediaFeedback,
        int ticketsComSugestaoIa,
        int ticketsComSlaVencido,
        IReadOnlyCollection<ReportCategoryCount> categorias)
    {
        TotalTickets = totalTickets;
        TicketsAguardando = ticketsAguardando;
        TicketsEmAndamento = ticketsEmAndamento;
        TicketsResolvidos = ticketsResolvidos;
        TicketsCriticos = ticketsCriticos;
        TicketsComFeedback = ticketsComFeedback;
        MediaFeedback = mediaFeedback;
        TicketsComSugestaoIa = ticketsComSugestaoIa;
        TicketsComSlaVencido = ticketsComSlaVencido;
        Categorias = categorias;
    }
}