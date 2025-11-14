using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SupportSystem.Infrastructure.Persistence.Migrations
{
    // Migração responsável por criar a tabela de base de conhecimento.
    public partial class AddKnowledgeBase : Migration
    {
        // Executa a criação da tabela KnowledgeBase e insere registros padrão.
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "KnowledgeBase",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Titulo = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Conteudo = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Categoria = table.Column<byte>(type: "tinyint", nullable: false, defaultValue: (byte)255),
                    PalavrasChave = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false, defaultValue: string.Empty),
                    AtualizadoEm = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KnowledgeBase", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "KnowledgeBase",
                columns: new[] { "Id", "AtualizadoEm", "Categoria", "Conteudo", "PalavrasChave", "Titulo" },
                values: new object[,]
                {
                    { 1, new DateTime(2024, 10, 5, 10, 0, 0, DateTimeKind.Utc), (byte)0, "Revisar políticas de acesso no AD, confirmar sincronização de credenciais e reiniciar o serviço VPN.", "vpn;credenciais;ad", "Erro de VPN após atualização de credenciais" },
                    { 2, new DateTime(2024, 9, 25, 13, 30, 0, DateTimeKind.Utc), (byte)2, "Validar utilização de recursos no servidor, analisar métricas de banco e aplicar índices recomendados.", "lentidao;aplicacao;sql", "Lentidão em aplicação web" },
                    { 3, new DateTime(2024, 9, 15, 8, 0, 0, DateTimeKind.Utc), (byte)3, "Executar backup completo, validar janela de manutenção e preparar plano de rollback documentado.", "atualizacao;critica;rollback", "Checklist de atualização crítica" }
                });
        }

        // Reverte a criação da tabela KnowledgeBase.
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "KnowledgeBase");
        }
    }
}
