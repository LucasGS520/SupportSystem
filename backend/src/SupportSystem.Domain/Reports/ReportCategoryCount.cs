using SupportSystem.Domain.Enums;

namespace SupportSystem.Domain.Reports;

// Representa a contagem agregada de tickets agrupados por categoria.
public sealed class ReportCategoryCount
{
    // Categoria de ticket considerada no agrupamento.
    public TicketCategory Categoria { get; }

    // Quantidade total de tickets da categoria.
    public int Quantidade { get; }

    // Cria o agregado com a categoria e a respectiva contagem consolidada.
    public ReportCategoryCount(TicketCategory categoria, int quantidade)
    {
        Categoria = categoria;
        Quantidade = quantidade;
    }
}