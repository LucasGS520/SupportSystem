using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SupportSystem.Application.DTOs;
using SupportSystem.Application.Interfaces;

namespace SupportSystem.Api.Controllers;

// Controlador responsável pelo fluxo de autenticação e registro de usuários.
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    // Serviço de domínio dedicado a autenticação e usuários.
    private readonly IUserService _userService;

    // Construtor que injeta o serviço de usuários.
    public AuthController(IUserService userService)
    {
        _userService = userService;
    }

    // Endpoint para registrar novos usuários na base.
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResult), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(AuthResult), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AuthResult>> Register(
        [FromBody] RegisterUserRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _userService.RegisterAsync(request, cancellationToken);
        if (!result.Sucesso)
        {
            return BadRequest(result);
        }

        return Created("api/auth/login", result);
    }

    // Endpoint para autenticar usuários existentes e emitir token JWT.
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(AuthResult), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResult>> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _userService.LoginAsync(request, cancellationToken);
        if (!result.Sucesso)
        {
            return Unauthorized(result);
        }

        return Ok(result);
    }
}
