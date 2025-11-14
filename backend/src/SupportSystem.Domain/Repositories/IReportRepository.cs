using System;
using System.Threading;
using System.Threading.Tasks;
using SupportSystem.Domain.Reports;

namespace SupportSystem.Domain.Repositories;

// Contrato para consultas agregadas de relatórios e indicadores.
public interface IReportRepository
{
    // Recupera o resumo consolidado dos tickets considerando o período informado.
    Task<ReportSummary> GetSummaryAsync(
        DateTime? dataInicial = null,
        DateTime? dataFinal = null,
        CancellationToken cancellationToken = default);
}
