using System;

namespace SupportSystem.Application.DTOs;

// Reúne indicadores consolidados para dashboards e relatórios gerenciais.
public sealed class ReportSummaryDto
{
    // Quantidade total de tickets avaliados na consulta.
    public int TotalTickets { get; init; }

    // Total de tickets aguardando tratativa.
    public int TicketsAguardando { get; init; }

    // Total de tickets em andamento.
    public int TicketsEmAndamento { get; init; }

    // Total de tickets resolvidos.
    public int TicketsResolvidos { get; init; }

    // Total de tickets classificados como críticos.
    public int TicketsCriticos { get; init; }

    // Quantidade de tickets com feedback registrado.
    public int TicketsComFeedback { get; init; }

    // Quantidade de tickets sem qualquer feedback.
    public int TicketsSemFeedback { get; init; }

    // Média aritmética das notas de feedback dos solicitantes.
    public double? MediaFeedback { get; init; }

    // Total de tickets com sugestões automáticas geradas pela IA.
    public int TicketsComSugestaoIa { get; init; }

    // Total de tickets cujo SLA está vencido e ainda não foram resolvidos.
    public int TicketsComSlaVencido { get; init; }

    // Distribuição de tickets por categoria.
    public IReadOnlyCollection<ReportCategoryBreakdownDto> TicketsPorCategoria { get; init; }
        = Array.Empty<ReportCategoryBreakdownDto>();
}
