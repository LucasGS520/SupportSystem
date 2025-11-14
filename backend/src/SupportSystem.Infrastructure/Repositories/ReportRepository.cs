using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SupportSystem.Domain.Enums;
using SupportSystem.Domain.Repositories;
using SupportSystem.Domain.Reports;
using SupportSystem.Infrastructure.Persistence;

namespace SupportSystem.Infrastructure.Repositories;

// Executa consultas SQL otimizadas para gerar relatórios consolidados.
public sealed class ReportRepository : IReportRepository
{
    private readonly SupportSystemContext _context;
    private readonly ILogger<ReportRepository> _logger;

    // Inicializa o repositório com o contexto do banco e o logger configurado.
    public ReportRepository(SupportSystemContext context, ILogger<ReportRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    // Executa as consultas agregadas utilizando um único round-trip no banco.
    public async Task<ReportSummary> GetSummaryAsync(
        DateTime? dataInicial = null,
        DateTime? dataFinal = null,
        CancellationToken cancellationToken = default)
    {
        await using var connection = _context.Database.GetDbConnection();
        if (connection.State != ConnectionState.Open)
        {
            await connection.OpenAsync(cancellationToken);
        }

        await using var command = connection.CreateCommand();
        command.CommandText =
            @"SELECT
                COUNT(1) AS TotalTickets,
                SUM(CASE WHEN [Status] = @statusAguardando THEN 1 ELSE 0 END) AS TicketsAguardando,
                SUM(CASE WHEN [Status] = @statusEmAndamento THEN 1 ELSE 0 END) AS TicketsEmAndamento,
                SUM(CASE WHEN [Status] = @statusResolvido THEN 1 ELSE 0 END) AS TicketsResolvidos,
                SUM(CASE WHEN [Status] = @statusCritico THEN 1 ELSE 0 END) AS TicketsCriticos,
                SUM(CASE WHEN FeedbackNota IS NOT NULL THEN 1 ELSE 0 END) AS TicketsComFeedback,
                AVG(CAST(FeedbackNota AS FLOAT)) AS MediaFeedback,
                SUM(CASE WHEN SugestaoIa IS NOT NULL AND LTRIM(RTRIM(SugestaoIa)) <> '' THEN 1 ELSE 0 END) AS TicketsComSugestaoIa,
                SUM(CASE WHEN SlaTarget IS NOT NULL AND SlaTarget < SYSUTCDATETIME() AND [Status] <> @statusResolvido THEN 1 ELSE 0 END) AS TicketsComSlaVencido
            FROM Tickets
            WHERE (@dataInicial IS NULL OR AbertoEm >= @dataInicial)
              AND (@dataFinal IS NULL OR AbertoEm <= @dataFinal);

            SELECT Categoria, COUNT(1) AS Quantidade
            FROM Tickets
            WHERE (@dataInicial IS NULL OR AbertoEm >= @dataInicial)
              AND (@dataFinal IS NULL OR AbertoEm <= @dataFinal)
            GROUP BY Categoria;";

        // Mapeia os parâmetros uma única vez reutilizando nas duas consultas.
        AddParameter(command, "@statusAguardando", (byte)TicketStatus.Aguardando);
        AddParameter(command, "@statusEmAndamento", (byte)TicketStatus.EmAndamento);
        AddParameter(command, "@statusResolvido", (byte)TicketStatus.Resolvido);
        AddParameter(command, "@statusCritico", (byte)TicketStatus.Critico);
        AddParameter(command, "@dataInicial", dataInicial, DbType.DateTime2);
        AddParameter(command, "@dataFinal", dataFinal, DbType.DateTime2);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        if (!await reader.ReadAsync(cancellationToken))
        {
            _logger.LogWarning("Consulta de relatórios não retornou dados");
            return new ReportSummary(0, 0, 0, 0, 0, 0, null, 0, 0, Array.Empty<ReportCategoryCount>());
        }

        var totalTickets = reader.IsDBNull(0) ? 0 : reader.GetInt32(0);
        var ticketsAguardando = reader.IsDBNull(1) ? 0 : reader.GetInt32(1);
        var ticketsEmAndamento = reader.IsDBNull(2) ? 0 : reader.GetInt32(2);
        var ticketsResolvidos = reader.IsDBNull(3) ? 0 : reader.GetInt32(3);
        var ticketsCriticos = reader.IsDBNull(4) ? 0 : reader.GetInt32(4);
        var ticketsComFeedback = reader.IsDBNull(5) ? 0 : reader.GetInt32(5);
        var mediaFeedback = reader.IsDBNull(6) ? null : reader.GetDouble(6);
        var ticketsComSugestaoIa = reader.IsDBNull(7) ? 0 : reader.GetInt32(7);
        var ticketsComSlaVencido = reader.IsDBNull(8) ? 0 : reader.GetInt32(8);

        var categorias = new List<ReportCategoryCount>();

        // Avança para o segundo result set com a distribuição por categoria.
        if (await reader.NextResultAsync(cancellationToken))
        {
            while (await reader.ReadAsync(cancellationToken))
            {
                var categoriaRaw = reader.IsDBNull(0) ? (byte)TicketCategory.Outros : reader.GetByte(0);
                var quantidade = reader.IsDBNull(1) ? 0 : reader.GetInt32(1);

                // Garante que valores inválidos sejam tratados como "Outros" para evitar exceções.
                var categoriaEnum = Enum.IsDefined(typeof(TicketCategory), categoriaRaw)
                    ? (TicketCategory)categoriaRaw
                    : TicketCategory.Outros;

                categorias.Add(new ReportCategoryCount(categoriaEnum, quantidade));
            }
        }

        return new ReportSummary(
            totalTickets,
            ticketsAguardando,
            ticketsEmAndamento,
            ticketsResolvidos,
            ticketsCriticos,
            ticketsComFeedback,
            mediaFeedback,
            ticketsComSugestaoIa,
            ticketsComSlaVencido,
            categorias);
    }

    private static void AddParameter(DbCommand command, string name, object? value, DbType? dbType = null)
    {
        var parameter = command.CreateParameter();
        parameter.ParameterName = name;
        parameter.Value = value ?? DBNull.Value;
        if (dbType.HasValue)
        {
            parameter.DbType = dbType.Value;
        }

        command.Parameters.Add(parameter);
    }
}
