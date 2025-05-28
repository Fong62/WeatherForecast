using Microsoft.AspNetCore.Mvc;

namespace WeatherForecast.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    private readonly ILogger<WeatherForecastController> _logger;

    public WeatherForecastController(ILogger<WeatherForecastController> logger)
    {
        _logger = logger;
        _logger.LogInformation("WeatherForecastController constructor called.");
    }

    [HttpGet(Name = "GetWeatherForecast")]
    public IEnumerable<WeatherForecast> Get()
    {
        var currentDate = DateTime.Now;

        var forecasts = Enumerable.Range(1, 5).Select(index =>
        {

            int tempC = Random.Shared.Next(-20, 55);
            if (tempC > 30)
            {
                _logger.LogWarning($"High temperature detected: {tempC}°C");
            }

            return new WeatherForecast
            {
                Date = DateOnly.FromDateTime(currentDate.AddDays(index)),
                TemperatureC = tempC,
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            };
        })
        .ToArray();

        if (forecasts.Any())
        {
            _logger.LogInformation($"Successfully generated {forecasts.Length} weather forecasts.");
        }
        else
        {
            _logger.LogWarning("No forecasts generated.");
        }

        return forecasts;
    }
}