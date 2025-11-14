using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SupportSystem.Application.DTOs;
using SupportSystem.Application.Interfaces;

namespace SupportSystem.Api.Controllers;

// Disponibiliza endpoints de relatórios e indicadores consolidados.
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;

    // Cria o controlador recebendo o serviço especializado em relatórios.
    public ReportsController(IReportService reportService)
    {
        _reportService = reportService;
    }

    // Retorna o resumo de KPIs considerando, opcionalmente, um recorte por período.
    [HttpGet("summary")]
    [ProducesResponseType(typeof(ReportSummaryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ReportSummaryDto>> GetSummary(
        [FromQuery] DateTime? dataInicial,
        [FromQuery] DateTime? dataFinal,
        CancellationToken cancellationToken)
    {
        // Valida o intervalo solicitado para evitar consultas sem sentido ou dados inconsistentes.
        if (dataInicial.HasValue && dataFinal.HasValue && dataInicial > dataFinal)
        {
            return BadRequest("O período informado é inválido: data inicial superior à data final.");
        }

        var resumo = await _reportService.GetSummaryAsync(dataInicial, dataFinal, cancellationToken);
        return Ok(resumo);
    }
}
