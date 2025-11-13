using Microsoft.EntityFrameworkCore;
using SupportSystem.Application.Interfaces;
using SupportSystem.Application.Services;
using SupportSystem.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { 
        Title = "Support System API", 
        Version = "v1",
        Description = "API de suporte/tickets com IA para classificação e priorização automática"
    });
});

// Configure Database (using In-Memory for development)
builder.Services.AddDbContext<SupportSystemDbContext>(options =>
    options.UseInMemoryDatabase("SupportSystemDb"));

// Register Application Services
builder.Services.AddScoped<ITicketService, TicketService>();
builder.Services.AddScoped<ITicketClassificationService, TicketClassificationService>();

// Configure CORS for multi-platform access (web, desktop, mobile)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
});

var app = builder.Build();

// Seed database with sample data
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<SupportSystemDbContext>();
    SeedData(context);
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

app.Run();

static void SeedData(SupportSystemDbContext context)
{
    if (context.Customers.Any())
        return;

    // Seed sample customers
    var customers = new[]
    {
        new SupportSystem.Domain.Entities.Customer 
        { 
            Name = "João Silva", 
            Email = "joao@example.com", 
            Phone = "+55 11 99999-9999",
            Company = "Tech Corp",
            CreatedAt = DateTime.UtcNow,
            DataProcessingConsent = true,
            ConsentDate = DateTime.UtcNow
        },
        new SupportSystem.Domain.Entities.Customer 
        { 
            Name = "Maria Santos", 
            Email = "maria@example.com", 
            Phone = "+55 11 88888-8888",
            Company = "Digital Solutions",
            CreatedAt = DateTime.UtcNow,
            DataProcessingConsent = true,
            ConsentDate = DateTime.UtcNow
        }
    };
    context.Customers.AddRange(customers);

    // Seed sample users (support agents)
    var users = new[]
    {
        new SupportSystem.Domain.Entities.User
        {
            Username = "admin",
            Email = "admin@support.com",
            PasswordHash = "hashed_password",
            FullName = "Administrator",
            Role = "Admin",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        },
        new SupportSystem.Domain.Entities.User
        {
            Username = "agent1",
            Email = "agent1@support.com",
            PasswordHash = "hashed_password",
            FullName = "Pedro Oliveira",
            Role = "Agent",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        }
    };
    context.Users.AddRange(users);

    // Seed sample knowledge base articles
    var articles = new[]
    {
        new SupportSystem.Domain.Entities.KnowledgeBaseArticle
        {
            Title = "Como resetar sua senha",
            Content = "Para resetar sua senha: 1) Clique em 'Esqueci minha senha' na tela de login. 2) Digite seu email. 3) Siga as instruções enviadas por email.",
            Category = SupportSystem.Domain.Enums.TicketCategory.Account,
            Tags = new[] { "senha", "login", "acesso" },
            IsPublished = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "admin"
        },
        new SupportSystem.Domain.Entities.KnowledgeBaseArticle
        {
            Title = "Resolvendo erros de conexão",
            Content = "Se você está tendo problemas de conexão: 1) Verifique sua internet. 2) Limpe o cache do navegador. 3) Tente outro navegador.",
            Category = SupportSystem.Domain.Enums.TicketCategory.Technical,
            Tags = new[] { "conexão", "erro", "rede" },
            IsPublished = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "admin"
        }
    };
    context.KnowledgeBaseArticles.AddRange(articles);

    context.SaveChanges();
}
