namespace SupportSystem.Application.DTOs;

// Expõe a quantidade de tickets por categoria para consumo do frontend.
public sealed class ReportCategoryBreakdownDto
{
    // Nome amigável da categoria agregada.
    public string Categoria { get; init; } = string.Empty;

    // Valor numérico total associado à categoria.
    public int Quantidade { get; init; }
}