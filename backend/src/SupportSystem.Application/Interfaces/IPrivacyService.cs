using SupportSystem.Application.DTOs;

namespace SupportSystem.Application.Interfaces;

// Define operações relacionadas a privacidade, exportação e retenção de dados.
public interface IPrivacyService
{
    // Exporta os dados completos do usuário autenticado.
    Task<UserDataExportDto?> ExportAsync(int userId, CancellationToken cancellationToken = default);

    // Remove todos os registros associados ao usuário, garantindo o direito de exclusão.
    Task<bool> DeleteAsync(int userId, CancellationToken cancellationToken = default);

    // Aplica a política de retenção apagando registros antigos automaticamente.
    Task<int> ApplyRetentionPolicyAsync(CancellationToken cancellationToken = default);
}