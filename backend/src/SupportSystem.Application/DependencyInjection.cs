using Microsoft.Extensions.DependencyInjection;
using SupportSystem.Application.Interfaces;
using SupportSystem.Application.Services;

namespace SupportSystem.Application;

// Extensões que encapsulam o registro dos serviços da camada de aplicação.
public static class DependencyInjection
{
    // Registra serviços da aplicação no container principal.
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ITicketService, TicketService>();
        services.AddScoped<IUserService, UserService>();
        services.AddHttpClient<IAIService, AIService>();
        return services;
    }
}
