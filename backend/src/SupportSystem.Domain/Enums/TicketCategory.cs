namespace SupportSystem.Domain.Enums;

// Define as categorias disponíveis para classificação e roteamento dos tickets.
public enum TicketCategory : byte
{
    // Incidentes relacionados a infraestrutura de redes, links ou VPN.
    Redes = 0,

    // Demandas envolvendo dispositivos do usuário final.
    SuporteAoUsuario = 1,

    // Questões aplicacionais, erros lógicos ou bugs reportados.
    Aplicacoes = 2,

    // Solicitações voltadas à segurança da informação.
    Seguranca = 3,

    // Categoria genérica utilizada até classificação refinada.
    Outros = 255
}
