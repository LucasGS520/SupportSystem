namespace SupportSystem.Domain.Entities;

// Representa um usuário autenticável do sistema de suporte.
public class User
{
    // Identificador único do usuário.
    public int Id { get; set; }

    // Nome completo utilizado para exibição.
    public string Nome { get; set; } = string.Empty;

    // E-mail corporativo utilizado no login.
    public string Email { get; set; } = string.Empty;

    // Hash com sal aplicado sobre a senha do usuário.
    public string SenhaHash { get; set; } = string.Empty;

    // Perfil funcional que determina autorizações.
    public string Papel { get; set; } = "user";

    // Registro de criação do usuário em UTC.
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;

    // Indica se o usuário autorizou o tratamento de dados pessoais.
    public bool ConsentimentoDados { get; set; }

    // Momento em que o consentimento foi registrado, em UTC.
    public DateTime? ConsentimentoRegistradoEm { get; set; }
}
