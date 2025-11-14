using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SupportSystem.Application.DTOs;
using SupportSystem.Application.Interfaces;
using SupportSystem.Domain.Repositories;

namespace SupportSystem.Application.Services;

// Implementa consultas de relatórios consolidados utilizando os repositórios otimizados.
public sealed class ReportService : IReportService
{
    private readonly IReportRepository _reportRepository;
    private readonly ILogger<ReportService> _logger;


    // Cria o serviço injetando o repositório e o logger para rastreabilidade.
    public ReportService(IReportRepository reportRepository, ILogger<ReportService> logger)
    {
        _reportRepository = reportRepository;
        _logger = logger;
    }

    // Obtém o resumo consolidado de métricas aplicando os filtros informados.
    public async Task<ReportSummaryDto> GetSummaryAsync(
        DateTime? dataInicial = null,
        DateTime? dataFinal = null,
        CancellationToken cancellationToken = default)
    {
        // Registra a consulta para facilitar a auditoria das métricas consumidas.
        _logger.LogInformation(
            "Gerando relatório consolidado entre {DataInicial} e {DataFinal}",
            dataInicial,
            dataFinal);

        var summary = await _reportRepository.GetSummaryAsync(dataInicial, dataFinal, cancellationToken);

        // Converte as estruturas de domínio para DTOs amigáveis ao frontend.
        return new ReportSummaryDto
        {
            TotalTickets = summary.TotalTickets,
            TicketsAguardando = summary.TicketsAguardando,
            TicketsEmAndamento = summary.TicketsEmAndamento,
            TicketsResolvidos = summary.TicketsResolvidos,
            TicketsCriticos = summary.TicketsCriticos,
            TicketsComFeedback = summary.TicketsComFeedback,
            TicketsSemFeedback = summary.TicketsSemFeedback,
            MediaFeedback = summary.MediaFeedback,
            TicketsComSugestaoIa = summary.TicketsComSugestaoIa,
            TicketsComSlaVencido = summary.TicketsComSlaVencido,
            TicketsPorCategoria = summary.Categorias
                .Select(categoria => new ReportCategoryBreakdownDto
                {
                    Categoria = categoria.Categoria.ToString(),
                    Quantidade = categoria.Quantidade
                })
                .OrderByDescending(dto => dto.Quantidade)
                .ToArray()
        };
    }
}
