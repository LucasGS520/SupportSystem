using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SupportSystem.Infrastructure.Persistence.Migrations
{
    // Migração inicial que cria a tabela de tickets e insere dados seed.
    public partial class InitialCreate : Migration
    {
        // Executa as instruções de criação da tabela e carga inicial.
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Tickets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Titulo = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Prioridade = table.Column<byte>(type: "tinyint", nullable: false),
                    Status = table.Column<byte>(type: "tinyint", nullable: false),
                    Solicitante = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    AbertoEm = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    SugestaoIa = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tickets", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Tickets",
                columns: new[] { "Id", "AbertoEm", "Prioridade", "Solicitante", "Status", "SugestaoIa", "Titulo" },
                values: new object[,]
                {
                    { 1, new DateTime(2024, 10, 10, 12, 30, 0, 0, DateTimeKind.Utc), (byte)3, "Maria Silva", (byte)2, "Verificar credenciais de AD, política de acesso e status do servidor TS.", "Erro ao acessar sistema via VPN" },
                    { 2, new DateTime(2024, 10, 9, 11, 15, 0, 0, DateTimeKind.Utc), (byte)2, "Depto RH", (byte)1, "Conferir uso de CPU, memória do servidor e índices do banco.", "Lentidão no sistema de folha" },
                    { 3, new DateTime(2024, 10, 8, 8, 45, 0, 0, DateTimeKind.Utc), (byte)3, "Diretoria TI", (byte)4, "Planejar janela de manutenção, snapshot do banco e rollback script.", "Atualização crítica do ERP" }
                });
        }

        // Desfaz as alterações desta migração removendo a tabela.
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Tickets");
        }
    }
}
