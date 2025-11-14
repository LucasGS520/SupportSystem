namespace SupportSystem.Application.DTOs;

// Resultado padrão de operações de autenticação.
public record AuthResult(bool Sucesso, string? Token, string? Mensagem);
