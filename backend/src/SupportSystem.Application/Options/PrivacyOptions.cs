namespace SupportSystem.Application.Options;

// Define parâmetros de configuração relacionados à privacidade e retenção.
public class PrivacyOptions
{
    // Nome da seção utilizada no arquivo de configuração.
    public const string SectionName = "Privacy";

    // Quantidade de dias para retenção de tickets resolvidos.
    public int RetentionDays { get; set; } = 365;
}