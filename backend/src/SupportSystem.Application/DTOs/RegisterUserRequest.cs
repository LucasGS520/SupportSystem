namespace SupportSystem.Application.DTOs;

// Payload recebido para registrar um novo usuário.
public record RegisterUserRequest(string Nome, string Email, string Senha, bool ConsentimentoDados);
