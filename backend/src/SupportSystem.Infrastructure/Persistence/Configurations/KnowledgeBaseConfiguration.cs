using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SupportSystem.Domain.Entities;
using SupportSystem.Domain.Enums;

namespace SupportSystem.Infrastructure.Persistence.Configurations;

// Configuração fluente da entidade <see cref="KnowledgeBase"/> para persistência em banco relacional.
public class KnowledgeBaseConfiguration : IEntityTypeConfiguration<KnowledgeBase>
{
    // Define mapeamentos, restrições e dados iniciais da base de conhecimento.
    public void Configure(EntityTypeBuilder<KnowledgeBase> builder)
    {
        builder.ToTable("KnowledgeBase");

        builder.HasKey(k => k.Id);

        builder.Property(k => k.Id)
            .ValueGeneratedOnAdd();

        builder.Property(k => k.Titulo)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(k => k.Conteudo)
            .HasMaxLength(2000)
            .IsRequired();

        builder.Property(k => k.Categoria)
            .HasConversion<byte>()
            .HasDefaultValue(TicketCategory.Outros)
            .IsRequired();

        builder.Property(k => k.PalavrasChave)
            .HasMaxLength(500)
            .HasDefaultValue(string.Empty);

        builder.Property(k => k.AtualizadoEm)
            .HasDefaultValueSql("SYSUTCDATETIME()")
            .IsRequired();

        // Registros iniciais ajudam a demonstrar a sugestão automática durante o desenvolvimento.
        builder.HasData(
            new KnowledgeBase
            {
                Id = 1,
                Titulo = "Erro de VPN após atualização de credenciais",
                Conteudo = "Revisar políticas de acesso no AD, confirmar sincronização de credenciais e reiniciar o serviço VPN.",
                Categoria = TicketCategory.Redes,
                PalavrasChave = "vpn;credenciais;ad",
                AtualizadoEm = new DateTime(2024, 10, 5, 10, 0, 0, DateTimeKind.Utc)
            },
            new KnowledgeBase
            {
                Id = 2,
                Titulo = "Lentidão em aplicação web",
                Conteudo = "Validar utilização de recursos no servidor, analisar métricas de banco e aplicar índices recomendados.",
                Categoria = TicketCategory.Aplicacoes,
                PalavrasChave = "lentidao;aplicacao;sql",
                AtualizadoEm = new DateTime(2024, 9, 25, 13, 30, 0, DateTimeKind.Utc)
            },
            new KnowledgeBase
            {
                Id = 3,
                Titulo = "Checklist de atualização crítica",
                Conteudo = "Executar backup completo, validar janela de manutenção e preparar plano de rollback documentado.",
                Categoria = TicketCategory.Seguranca,
                PalavrasChave = "atualizacao;critica;rollback",
                AtualizadoEm = new DateTime(2024, 9, 15, 8, 0, 0, DateTimeKind.Utc)
            });
    }
}