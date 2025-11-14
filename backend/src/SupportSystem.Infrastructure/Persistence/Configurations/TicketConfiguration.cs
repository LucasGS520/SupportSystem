using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SupportSystem.Domain.Entities;
using SupportSystem.Domain.Enums;

namespace SupportSystem.Infrastructure.Persistence.Configurations;

// Configuração fluente da entidade <see cref="Ticket"/> no banco relacional.
public class TicketConfiguration : IEntityTypeConfiguration<Ticket>
{
    // Aplica restrições de coluna, conversões e dados seed para tickets.
    public void Configure(EntityTypeBuilder<Ticket> builder)
    {
        builder.ToTable("Tickets");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Titulo)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(t => t.Prioridade)
            .HasConversion<byte>()
            .IsRequired();

        builder.Property(t => t.Status)
            .HasConversion<byte>()
            .IsRequired();

        builder.Property(t => t.OwnerId)
            .IsRequired();

        builder.Property(t => t.AssignedTechnicianId);

        builder.Property(t => t.Categoria)
            .HasConversion<byte>()
            .HasDefaultValue(TicketCategory.Outros)
            .IsRequired();

        builder.Property(t => t.SlaTarget);

        builder.Property(t => t.Solicitante)
            .HasMaxLength(120);

        builder.Property(t => t.Descricao)
            .HasMaxLength(2000);

        builder.Property(t => t.AbertoEm)
            .HasDefaultValueSql("SYSUTCDATETIME()")
            .IsRequired();

        builder.Property(t => t.SugestaoIa)
            .HasMaxLength(500);

        builder.OwnsOne(t => t.Feedback, feedbackBuilder =>
        {
            feedbackBuilder.Property(f => f.Nota)
                .HasColumnName("FeedbackNota");

            feedbackBuilder.Property(f => f.Comentario)
                .HasMaxLength(500)
                .HasColumnName("FeedbackComentario");

            feedbackBuilder.Property(f => f.RegistradoEm)
                .HasColumnName("FeedbackRegistradoEm");
        });

        builder.HasData(
            new Ticket
            {
                Id = 1,
                Titulo = "Erro ao acessar sistema via VPN",
                Prioridade = TicketPriority.Alta,
                Status = TicketStatus.EmAndamento,
                OwnerId = 1,
                AssignedTechnicianId = 3,
                Categoria = TicketCategory.Redes,
                Descricao = "Usuária relata falha ao autenticar na VPN corporativa após troca de senha.",
                SlaTarget = new DateTime(2024, 10, 11, 12, 30, 0, DateTimeKind.Utc),
                Solicitante = "Maria Silva",
                AbertoEm = new DateTime(2024, 10, 10, 12, 30, 0, DateTimeKind.Utc),
                SugestaoIa = "Verificar credenciais de AD, política de acesso e status do servidor TS."
            },
            new Ticket
            {
                Id = 2,
                Titulo = "Lentidão no sistema de folha",
                Prioridade = TicketPriority.Media,
                Status = TicketStatus.Aguardando,
                OwnerId = 2,
                AssignedTechnicianId = null,
                Categoria = TicketCategory.Aplicacoes,
                Descricao = "Equipe de RH reporta demora ao calcular folha durante o fechamento do mês.",
                SlaTarget = new DateTime(2024, 10, 10, 17, 0, 0, DateTimeKind.Utc),
                Solicitante = "Depto RH",
                AbertoEm = new DateTime(2024, 10, 9, 11, 15, 0, DateTimeKind.Utc),
                SugestaoIa = "Conferir uso de CPU, memória do servidor e índices do banco."
            },
            new Ticket
            {
                Id = 3,
                Titulo = "Atualização crítica do ERP",
                Prioridade = TicketPriority.Alta,
                Status = TicketStatus.Critico,
                OwnerId = 3,
                AssignedTechnicianId = 4,
                Categoria = TicketCategory.Seguranca,
                Descricao = "Equipe de RH reporta demora ao calcular folha durante o fechamento do mês.",
                SlaTarget = new DateTime(2024, 10, 8, 20, 45, 0, DateTimeKind.Utc),
                Solicitante = "Diretoria TI",
                AbertoEm = new DateTime(2024, 10, 8, 8, 45, 0, DateTimeKind.Utc),
                SugestaoIa = "Planejar janela de manutenção, snapshot do banco e rollback script."
            });
    }
}
