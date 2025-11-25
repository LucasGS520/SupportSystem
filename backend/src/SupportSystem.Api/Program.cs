using System.IO;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Microsoft.OpenApi.Models;
using SupportSystem.Application;
using SupportSystem.Application.Options;
using SupportSystem.Infrastructure;
using SupportSystem.Infrastructure.Persistence;
using Microsoft.IdentityModel.Tokens;

// Inicializa o builder com as configurações padrão do ASP.NET Core.
var builder = WebApplication.CreateBuilder(args);

const string CorsPolicyName = "AllowFrontendClients";
var defaultCorsOrigins = new[]
{
    "http://localhost:4173",
    "http://localhost:5173",
    "http://127.0.0.1:4173"
};

// Configura os controladores e mantém os nomes originais das propriedades JSON.
builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
    });

// Adiciona recursos de documentação e descrição da API.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "SupportSystem API",
        Version = "v1",
        Description = "API REST do PIM ERP Suporte para gestão de chamados."
    });
});

// Registra camadas de aplicação e infraestrutura para injeção de dependência.
builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration);

builder.Services.Configure<PrivacyOptions>(builder.Configuration.GetSection(PrivacyOptions.SectionName));

// Carrega as opções de JWT da configuração e valida se o segredo está definido.
var jwtOptions = new JwtOptions();
builder.Configuration.GetSection(JwtOptions.SectionName).Bind(jwtOptions);
if (string.IsNullOrWhiteSpace(jwtOptions.Secret))
{
    throw new InvalidOperationException("Configuração JWT secreta não encontrada.");
}

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));

var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Secret));

var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>();

if (allowedOrigins is null || allowedOrigins.Length == 0)
{
    allowedOrigins = defaultCorsOrigins;
}

builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsPolicyName, policy =>
    {
        policy
            .WithOrigins(allowedOrigins)
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

// Habilita autenticação JWT com validações padrão.
builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidAudience = jwtOptions.Audience,
            IssuerSigningKey = signingKey
        };
    });

// Constrói o aplicativo configurado.
var app = builder.Build();

// Aplica migrações pendentes para garantir alinhamento do banco.
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<SupportSystemContext>();
    dbContext.Database.Migrate();
}

// Carrega o Swagger somente em ambientes locais e de desenvolvimento.
if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Local"))
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Determina o caminho do frontend para servir assets estáticos.
var frontendPath = Path.GetFullPath(
    Path.Combine(app.Environment.ContentRootPath, "..", "..", "..", "frontend"));
PhysicalFileProvider? frontendProvider = null;
var indexFilePath = Path.Combine(frontendPath, "index.html");

// Força comunicação HTTPS em todas as rotas.
app.UseHttpsRedirection();

// Publica arquivos do frontend quando o diretório estiver disponível.
if (Directory.Exists(frontendPath))
{
    frontendProvider = new PhysicalFileProvider(frontendPath);
    app.UseDefaultFiles(new DefaultFilesOptions
    {
        FileProvider = frontendProvider,
        RequestPath = ""
    });

    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = frontendProvider,
        RequestPath = ""
    });
}

// Habilita roteamento padrão para resolver endpoints.
app.UseRouting();

app.UseCors(CorsPolicyName);

// Habilita autenticação e autorização com JWT.
app.UseAuthentication();
app.UseAuthorization();

// Mapeia automaticamente os controladores REST.
app.MapControllers();

// Direciona qualquer rota desconhecida para o index do frontend.
if (frontendProvider is not null)
{
    app.MapFallback(async context =>
    {
        if (File.Exists(indexFilePath))
        {
            context.Response.ContentType = "text/html";
            await context.Response.SendFileAsync(indexFilePath);
        }
        else
        {
            context.Response.StatusCode = StatusCodes.Status404NotFound;
        }
    });
}

// Inicia a aplicação web.
app.Run();
