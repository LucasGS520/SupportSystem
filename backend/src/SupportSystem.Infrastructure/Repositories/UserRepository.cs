using Microsoft.EntityFrameworkCore;
using SupportSystem.Application.Abstractions;
using SupportSystem.Domain.Entities;
using SupportSystem.Domain.Repositories;
using SupportSystem.Infrastructure.Persistence;

namespace SupportSystem.Infrastructure.Repositories;

// Implementação EF Core para persistir entidades de usuário.
public class UserRepository : IUserRepository
{
    private readonly SupportSystemContext _context;
    private readonly ISensitiveDataProtector _protector;

    // Recebe o contexto configurado pela infraestrutura.
    public UserRepository(SupportSystemContext context, ISensitiveDataProtector protector)
    {
        _context = context;
        _protector = protector;
    }

    // Insere novo usuário e salva alterações.
    public async Task<User> AddAsync(User user, CancellationToken cancellationToken = default)
    {
        var entity = MapToStorage(user);
        await _context.Users.AddAsync(entity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return MapToDomain(entity);
    }

    // Localiza usuário pelo e-mail normalizado.
    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

        return entity is null ? null : MapToDomain(entity);
    }

    // Busca usuário pelo identificador.
    public async Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

        return entity is null ? null : MapToDomain(entity);
    }

    // Atualiza os dados do usuário existente.
    public async Task UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        var entity = MapToStorage(user);
        _context.Users.Update(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    // Remove um usuário e suas credenciais.
    public async Task DeleteAsync(User user, CancellationToken cancellationToken = default)
    {
        var entity = new User { Id = user.Id };
        _context.Users.Attach(entity);
        _context.Users.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    // Converte o usuário de domínio para o modelo protegido utilizado na persistência.
    private User MapToStorage(User user)
    {
        return new User
        {
            Id = user.Id,
            Nome = Protect(user.Nome) ?? string.Empty,
            Email = user.Email,
            SenhaHash = user.SenhaHash,
            Papel = user.Papel,
            CriadoEm = user.CriadoEm,
            ConsentimentoDados = user.ConsentimentoDados,
            ConsentimentoRegistradoEm = user.ConsentimentoRegistradoEm
        };
    }

    // Reconstrói o usuário descriptografado para uso na aplicação.
    private User MapToDomain(User entity)
    {
        return new User
        {
            Id = entity.Id,
            Nome = Unprotect(entity.Nome) ?? string.Empty,
            Email = entity.Email,
            SenhaHash = entity.SenhaHash,
            Papel = entity.Papel,
            CriadoEm = entity.CriadoEm,
            ConsentimentoDados = entity.ConsentimentoDados,
            ConsentimentoRegistradoEm = entity.ConsentimentoRegistradoEm
        };
    }

    // Aplica proteção no texto antes de persistir.
    private string? Protect(string? value) => _protector.Protect(value);

    // Remove a proteção aplicada nos dados persistidos.
    private string? Unprotect(string? value) => _protector.Unprotect(value);
}
