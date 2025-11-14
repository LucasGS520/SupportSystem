using System;
using System.IO;
using Microsoft.AspNetCore.DataProtection;
using SupportSystem.Application.Abstractions;

namespace SupportSystem.Infrastructure.Security;

// Implementa proteção de dados sensíveis com o mecanismo oficial do ASP.NET Core.
public class DataProtectionSensitiveDataProtector : ISensitiveDataProtector
{
    private readonly IDataProtector _protector;

    // Cria a instância utilizando o provedor informado.
    public DataProtectionSensitiveDataProtector(IDataProtectionProvider provider)
    {
        _protector = provider.CreateProtector("SupportSystem.SensitiveData");
    }

    // Constrói uma instância utilizando chaves ephemerais para cenários de design-time.
    public static DataProtectionSensitiveDataProtector CreateWithEphemeralKeys()
    {
        var keysDirectory = Path.Combine(AppContext.BaseDirectory, "dp-keys");
        var provider = DataProtectionProvider.Create(new DirectoryInfo(keysDirectory));
        return new DataProtectionSensitiveDataProtector(provider);
    }

    public string? Protect(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return value;
        }

        return _protector.Protect(value);
    }

    public string? Unprotect(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return value;
        }

        try
        {
            return _protector.Unprotect(value);
        }
        catch (Exception)
        {
            // Retornamos o valor bruto para evitar perda de dados quando as chaves mudam.
            return value;
        }
    }
}