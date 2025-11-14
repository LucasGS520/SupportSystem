using Microsoft.EntityFrameworkCore;
using SupportSystem.Domain.Entities;
using SupportSystem.Domain.Enums;
using SupportSystem.Domain.Repositories;
using SupportSystem.Infrastructure.Persistence;

namespace SupportSystem.Infrastructure.Repositories;

/// <summary>
/// Implementa buscas da base de conhecimento usando EF Core para apoiar sugestões automáticas.
/// </summary>
public class KnowledgeBaseRepository : IKnowledgeBaseRepository
{
    private readonly SupportSystemContext _context;

    /// <summary>
    /// Inicializa o repositório injetando o contexto de dados configurado.
    /// </summary>
    public KnowledgeBaseRepository(SupportSystemContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<KnowledgeBase>> BuscarRelevantesAsync(
        string termo,
        TicketCategory categoria,
        int limite = 5,
        CancellationToken cancellationToken = default)
    {
        var termoNormalizado = $"%{termo?.Trim()}%";

        // A consulta prioriza a categoria informada e aplica filtro textual simples por título e palavras-chave.
        var query = _context.KnowledgeBase
            .AsNoTracking()
            .Where(k => k.Categoria == categoria || categoria == TicketCategory.Outros)
            .Where(k => string.IsNullOrWhiteSpace(termo)
                || EF.Functions.Like(k.Titulo, termoNormalizado)
                || EF.Functions.Like(k.PalavrasChave, termoNormalizado))
            .OrderByDescending(k => k.AtualizadoEm)
            .ThenBy(k => k.Titulo)
            .Take(limite);

        return await query.ToListAsync(cancellationToken);
    }
}
