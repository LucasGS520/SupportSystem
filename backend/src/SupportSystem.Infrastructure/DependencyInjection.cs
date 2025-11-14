using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SupportSystem.Application.Abstractions;
using SupportSystem.Domain.Repositories;
using SupportSystem.Infrastructure.Persistence;
using SupportSystem.Infrastructure.Repositories;
using SupportSystem.Infrastructure.Time;

namespace SupportSystem.Infrastructure;

// Extensões responsáveis por registrar dependências de infraestrutura.
public static class DependencyInjection
{
    // Configura banco, repositórios e utilidades compartilhadas.
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Determina a string de conexão priorizando appsettings, variáveis e um fallback local.
        var connectionString = configuration.GetConnectionString("SupportSystem")
            ?? configuration["Database:ConnectionString"]
            ?? "Server=(localdb)\\MSSQLLocalDB;Database=SupportSystemDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";

        // Registra contexto EF Core apontando para SQL Server com assembly de migrações dedicado.
        services.AddDbContext<SupportSystemContext>(options =>
        {
            options.UseSqlServer(connectionString, sql =>
            {
                sql.MigrationsAssembly(typeof(SupportSystemContext).Assembly.FullName);
                sql.EnableRetryOnFailure();
            });
        });

        // Repositório e provedor de data/hora expostos para a aplicação.
        services.AddScoped<ITicketRepository, TicketRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IDateTimeProvider, SystemDateTimeProvider>();

        return services;
    }
}
