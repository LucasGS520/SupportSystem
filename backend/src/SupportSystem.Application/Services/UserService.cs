using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SupportSystem.Application.DTOs;
using SupportSystem.Application.Interfaces;
using SupportSystem.Application.Options;
using SupportSystem.Domain.Entities;
using SupportSystem.Domain.Repositories;

namespace SupportSystem.Application.Services;

// Centraliza as regras de autenticação e registro de usuários.
public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<UserService> _logger;
    private readonly JwtOptions _jwtOptions;

    // Injeta repositório, logger e opções de JWT.
    public UserService(
        IUserRepository userRepository,
        ILogger<UserService> logger,
        IOptions<JwtOptions> jwtOptions)
    {
        _userRepository = userRepository;
        _logger = logger;
        _jwtOptions = jwtOptions.Value;
    }

    // Cria um usuário novo após validar e-mail e aplicar hash à senha.
    public async Task<AuthResult> RegisterAsync(RegisterUserRequest request, CancellationToken cancellationToken = default)
    {
        if (!request.ConsentimentoDados)
        {
            // Bloqueia cadastro sem consentimento explícito para aderir à LGPD.
            return new AuthResult(false, null, "Consentimento é obrigatório para criar a conta.");
        }

        var normalizedEmail = NormalizeEmail(request.Email);
        var existingUser = await _userRepository.GetByEmailAsync(normalizedEmail, cancellationToken);
        if (existingUser is not null)
        {
            _logger.LogWarning("Tentativa de registro com e-mail já utilizado: {Email}", normalizedEmail);
            return new AuthResult(false, null, "E-mail já cadastrado.");
        }

        var consentTime = DateTime.UtcNow;

        var user = new User
        {
            Nome = request.Nome.Trim(),
            Email = normalizedEmail,
            SenhaHash = HashPassword(request.Senha),
            Papel = "user",
            CriadoEm = consentTime,
            ConsentimentoDados = request.ConsentimentoDados,
            ConsentimentoRegistradoEm = consentTime
        };

        var created = await _userRepository.AddAsync(user, cancellationToken);
        var token = GenerateJwtToken(created);

        _logger.LogInformation("Usuário {UserId} registrado com sucesso", created.Id);
        return new AuthResult(true, token, null);
    }

    // Valida credenciais fornecidas e retorna token quando válidas.
    public async Task<AuthResult> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = NormalizeEmail(request.Email);
        var user = await _userRepository.GetByEmailAsync(normalizedEmail, cancellationToken);
        if (user is null)
        {
            _logger.LogWarning("Login com e-mail inválido: {Email}", normalizedEmail);
            return new AuthResult(false, null, "Credenciais inválidas.");
        }

        var validPassword = VerifyPassword(request.Senha, user.SenhaHash);
        if (!validPassword)
        {
            _logger.LogWarning("Senha inválida para o usuário {UserId}", user.Id);
            return new AuthResult(false, null, "Credenciais inválidas.");
        }

        var token = GenerateJwtToken(user);
        _logger.LogInformation("Usuário {UserId} autenticado", user.Id);
        return new AuthResult(true, token, null);
    }

    // Gera o token JWT utilizando as opções configuradas.
    private string GenerateJwtToken(User user)
    {
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.Secret));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Nome),
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(JwtRegisteredClaimNames.UniqueName, user.Nome),
            new(ClaimTypes.Role, user.Papel)
        };

        var tokenDescriptor = new JwtSecurityToken(
            issuer: _jwtOptions.Issuer,
            audience: _jwtOptions.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtOptions.ExpirationMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
    }

    // Aplica PBKDF2 com sal aleatório para proteger a senha.
    private string HashPassword(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(16);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, 100_000, HashAlgorithmName.SHA256, 32);
        return Convert.ToBase64String(salt) + ":" + Convert.ToBase64String(hash);
    }

    // Compara senha informada com o hash persistido.
    private bool VerifyPassword(string password, string storedHash)
    {
        var parts = storedHash.Split(':');
        if (parts.Length != 2)
        {
            _logger.LogError("Hash de senha em formato inválido");
            return false;
        }

        var salt = Convert.FromBase64String(parts[0]);
        var expectedHash = Convert.FromBase64String(parts[1]);
        var hashToCompare = Rfc2898DeriveBytes.Pbkdf2(password, salt, 100_000, HashAlgorithmName.SHA256, expectedHash.Length);

        return CryptographicOperations.FixedTimeEquals(hashToCompare, expectedHash);
    }

    // Normaliza o e-mail para busca e comparação consistentes.
    private string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();
}
