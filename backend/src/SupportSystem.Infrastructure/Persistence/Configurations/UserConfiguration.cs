using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SupportSystem.Domain.Entities;

namespace SupportSystem.Infrastructure.Persistence.Configurations;

// Configuração fluente da entidade <see cref="User"/>.
public class UserConfiguration : IEntityTypeConfiguration<User>
{
    // Define restrições e índices para a tabela de usuários.
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(u => u.Id);

        builder.HasIndex(u => u.Email)
            .IsUnique();

        builder.Property(u => u.Nome)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(u => u.Email)
            .HasMaxLength(180)
            .IsRequired();

        builder.Property(u => u.SenhaHash)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(u => u.Papel)
            .HasMaxLength(60)
            .HasDefaultValue("user")
            .IsRequired();

        builder.Property(u => u.CriadoEm)
            .HasDefaultValueSql("SYSUTCDATETIME()")
            .IsRequired();
    }
}
