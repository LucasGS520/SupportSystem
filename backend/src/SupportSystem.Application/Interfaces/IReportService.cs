using System;
using System.Threading;
using System.Threading.Tasks;
using SupportSystem.Application.DTOs;

namespace SupportSystem.Application.Interfaces;

// Define operações de consulta utilizadas pelos relatórios gerenciais.
public interface IReportService
{
    // Obtém métricas consolidadas para o período informado.
    Task<ReportSummaryDto> GetSummaryAsync(
        DateTime? dataInicial = null,
        DateTime? dataFinal = null,
        CancellationToken cancellationToken = default);
}