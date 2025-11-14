using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SupportSystem.Application.DTOs;
using SupportSystem.Application.Interfaces;

namespace SupportSystem.Api.Controllers;

// Expõe endpoints relacionados a privacidade, LGPD e retenção de dados.
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PrivacyController : ControllerBase
{
    private readonly IPrivacyService _privacyService;
    private readonly ILogger<PrivacyController> _logger;

    // Inicializa o controlador injetando o serviço de privacidade.
    public PrivacyController(IPrivacyService privacyService, ILogger<PrivacyController> logger)
    {
        _privacyService = privacyService;
        _logger = logger;
    }

    // Exporta todos os dados pessoais associados ao usuário autenticado.
    [HttpGet("export")]
    [ProducesResponseType(typeof(UserDataExportDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserDataExportDto>> ExportAsync(CancellationToken cancellationToken)
    {
        var userId = ResolveUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var export = await _privacyService.ExportAsync(userId.Value, cancellationToken);
        if (export is null)
        {
            return NotFound();
        }

        return Ok(export);
    }

    // Remove os dados do usuário autenticado garantindo o direito ao esquecimento.
    [HttpDelete("forget-me")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ForgetMeAsync(CancellationToken cancellationToken)
    {
        var userId = ResolveUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var removed = await _privacyService.DeleteAsync(userId.Value, cancellationToken);
        return removed ? NoContent() : NotFound();
    }

    // Executa a política de retenção removendo registros antigos.
    [HttpPost("retention/apply")]
    [Authorize(Roles = "admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ApplyRetentionAsync(CancellationToken cancellationToken)
    {
        var removed = await _privacyService.ApplyRetentionPolicyAsync(cancellationToken);
        _logger.LogInformation("Retenção manual executada com remoção de {Count} tickets", removed);
        return Ok(new { removidos = removed });
    }

    // Recupera o identificador do usuário logado através do token JWT.
    private int? ResolveUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)
                    ?? User.FindFirst(ClaimTypes.Sid)
                    ?? User.FindFirst(ClaimTypes.Actor)
                    ?? User.FindFirst("sub");

        if (claim is null)
        {
            return null;
        }

        return int.TryParse(claim.Value, out var userId) ? userId : null;
    }
}
