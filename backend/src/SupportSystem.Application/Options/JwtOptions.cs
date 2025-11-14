namespace SupportSystem.Application.Options;

// Representa as configurações necessárias para emissão de tokens JWT.
public class JwtOptions
{
    // Nome padrão da seção no arquivo de configuração.
    public const string SectionName = "Jwt";

    // Segredo simétrico utilizado para assinatura.
    public string Secret { get; init; } = string.Empty;

    // Emissor padrão registrado no token.
    public string Issuer { get; init; } = string.Empty;

    // Público-alvo aceito para validação.
    public string Audience { get; init; } = string.Empty;

    // Duração do token em minutos.
    public int ExpirationMinutes { get; init; } = 60;
}
