using SupportSystem.Domain.Entities;
using SupportSystem.Domain.Enums;

namespace SupportSystem.Domain.Repositories;

// Define operações de leitura da base de conhecimento usadas para enriquecer chamados.
public interface IKnowledgeBaseRepository
{
    // Retorna artigos alinhados ao contexto informado ordenados por relevância.
    Task<IReadOnlyList<KnowledgeBase>> BuscarRelevantesAsync(
        string termo,
        TicketCategory categoria,
        int limite = 5,
        CancellationToken cancellationToken = default);
}