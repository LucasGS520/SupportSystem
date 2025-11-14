using Microsoft.AspNetCore.Mvc;

namespace SupportSystem.Api.Controllers;

// Controlador de exemplo que expõe previsões meteorológicas.
[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    // Lista fixa de descrições para as previsões retornadas.
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    // Logger padrão utilizado para rastrear requisições.
    private readonly ILogger<WeatherForecastController> _logger;

    // Construtor que injeta o logger configurado.
    public WeatherForecastController(ILogger<WeatherForecastController> logger)
    {
        _logger = logger;
    }

    // Retorna uma lista fictícia de previsões climáticas.
    [HttpGet(Name = "GetWeatherForecast")]
    public IEnumerable<WeatherForecast> Get()
    {
        return Enumerable.Range(1, 5).Select(index => new WeatherForecast
        {
            Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        })
        .ToArray();
    }
}
