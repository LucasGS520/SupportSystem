using Microsoft.EntityFrameworkCore;
using SupportSystem.Domain.Entities;

namespace SupportSystem.Infrastructure.Persistence;

// Contexto EF Core que orquestra o mapeamento das entidades do domínio.
public class SupportSystemContext(DbContextOptions<SupportSystemContext> options) : DbContext(options)
{
    // DbSet que representa a tabela de tickets.
    public DbSet<Ticket> Tickets => Set<Ticket>();

    // DbSet que representa a tabela de usuários.
    public DbSet<User> Users => Set<User>();

    // Aplica configurações fluentes localizadas no assembly da infraestrutura.
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SupportSystemContext).Assembly);
    }
}
