using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace SupportSystem.Infrastructure.Persistence;

// Factory utilizada por ferramentas de CLI para criar o contexto em tempo de design.
public class SupportSystemContextFactory : IDesignTimeDbContextFactory<SupportSystemContext>
{
    // Constrói o contexto com uma connection string local para gerar migrações.
    public SupportSystemContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<SupportSystemContext>();
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile($"appsettings.{environment}.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("SupportSystem")
            ?? configuration["Database:ConnectionString"]
            ?? "Server=(localdb)\\MSSQLLocalDB;Database=SupportSystemDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";

        optionsBuilder.UseSqlServer(connectionString, sql =>
        {
            sql.MigrationsAssembly(typeof(SupportSystemContext).Assembly.FullName);
        });

        return new SupportSystemContext(optionsBuilder.Options);
    }
}
