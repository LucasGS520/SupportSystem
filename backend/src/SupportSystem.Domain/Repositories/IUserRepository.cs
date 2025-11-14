using SupportSystem.Domain.Entities;

namespace SupportSystem.Domain.Repositories;

// Contrato de persistência para manipulação de usuários.
public interface IUserRepository
{
    // Busca um usuário pelo identificador único.
    Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    // Recupera um usuário pelo e-mail normalizado.
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    // Insere um novo usuário no repositório.
    Task<User> AddAsync(User user, CancellationToken cancellationToken = default);
}
