using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace SupportSystem.Infrastructure.Persistence;

// Factory utilizada por ferramentas de CLI para criar o contexto em tempo de design.
public class SupportSystemContextFactory : IDesignTimeDbContextFactory<SupportSystemContext>
{
    // Constrói o contexto com uma connection string local para gerar migrações.
    public SupportSystemContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<SupportSystemContext>();
        var connectionString =
            "Server=(localdb)\\MSSQLLocalDB;Database=SupportSystemDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";

        optionsBuilder.UseSqlServer(connectionString);

        return new SupportSystemContext(optionsBuilder.Options);
    }
}
