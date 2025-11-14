using Microsoft.EntityFrameworkCore;
using SupportSystem.Domain.Entities;
using SupportSystem.Domain.Repositories;
using SupportSystem.Infrastructure.Persistence;

namespace SupportSystem.Infrastructure.Repositories;

// Implementação EF Core para persistir entidades de usuário.
public class UserRepository : IUserRepository
{
    private readonly SupportSystemContext _context;

    // Recebe o contexto configurado pela infraestrutura.
    public UserRepository(SupportSystemContext context)
    {
        _context = context;
    }

    // Insere novo usuário e salva alterações.
    public async Task<User> AddAsync(User user, CancellationToken cancellationToken = default)
    {
        await _context.Users.AddAsync(user, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return user;
    }

    // Localiza usuário pelo e-mail normalizado.
    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
    }

    // Busca usuário pelo identificador.
    public async Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Users.FindAsync(new object?[] { id }, cancellationToken);
    }
}
