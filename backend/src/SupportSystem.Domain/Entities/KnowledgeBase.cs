using SupportSystem.Domain.Enums;

namespace SupportSystem.Domain.Entities;

// Representa um artigo ou instrução operacional disponível na base de conhecimento interna.
public class KnowledgeBase
{
    // Identificador único do artigo armazenado.
    public int Id { get; set; }

    // Título curto e objetivo utilizado na listagem e busca.
    public string Titulo { get; set; } = string.Empty;

    // Conteúdo detalhado da solução ou procedimento recomendado.
    public string Conteudo { get; set; } = string.Empty;

    // Categoria primária relacionada ao artigo para auxiliar no roteamento das sugestões.
    public TicketCategory Categoria { get; set; } = TicketCategory.Outros;

    // Palavras-chave auxiliares para facilitar a busca textual.
    public string PalavrasChave { get; set; } = string.Empty;

    // Data da última atualização em UTC utilizada para ordenar relevância e auditoria.
    public DateTime AtualizadoEm { get; set; } = DateTime.UtcNow;
}