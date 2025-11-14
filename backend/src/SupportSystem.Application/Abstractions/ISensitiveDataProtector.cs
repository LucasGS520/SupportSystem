namespace SupportSystem.Application.Abstractions;

// Define operações de proteção e desproteção para campos sensíveis conforme LGPD.
public interface ISensitiveDataProtector
{
    // Protege o valor informado antes da persistência ou transporte.
    string? Protect(string? value);

    // Remove a proteção aplicada sobre o valor informado.
    string? Unprotect(string? value);
}