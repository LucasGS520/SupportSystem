using SupportSystem.Application.DTOs;

namespace SupportSystem.Application.Interfaces;

// Expõe operações para autenticação e manutenção de usuários.
public interface IUserService
{
    // Registra um novo usuário e devolve token de acesso quando bem-sucedido.
    Task<AuthResult> RegisterAsync(RegisterUserRequest request, CancellationToken cancellationToken = default);

    // Realiza login validando credenciais e emitindo novo token.
    Task<AuthResult> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
}
