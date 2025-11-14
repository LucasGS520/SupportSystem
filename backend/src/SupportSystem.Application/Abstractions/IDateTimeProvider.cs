namespace SupportSystem.Application.Abstractions;

// Abstrai o acesso à data/hora para facilitar testes e customizações.
public interface IDateTimeProvider
{
    // Data e hora atuais no fuso UTC.
    DateTime UtcNow { get; }
}
