using SupportSystem.Application.Abstractions;

namespace SupportSystem.Infrastructure.Time;

// Provedor que acessa o relógio do sistema para obter UTC.
public class SystemDateTimeProvider : IDateTimeProvider
{
    // Retorna a data/hora atual em UTC diretamente do sistema operacional.
    public DateTime UtcNow => DateTime.UtcNow;
}
