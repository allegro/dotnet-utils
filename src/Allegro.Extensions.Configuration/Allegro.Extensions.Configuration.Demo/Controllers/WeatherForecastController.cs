using Microsoft.AspNetCore.Mvc;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Allegro.Extensions.Configuration.Demo.Controllers;

public record WeatherForecast(DateTime Date, int TemperatureC, string Summary);

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    [HttpGet(Name = "GetWeatherForecast")]
    public Task<IEnumerable<WeatherForecast>> Get()
    {
        return Task.FromResult<IEnumerable<WeatherForecast>>(
            Enumerable.Range(1, 5).Select(
                    index => new WeatherForecast(
                        DateTime.Now.AddDays(index),
                        Random.Shared.Next(-20, 55),
                        Summaries[Random.Shared.Next(Summaries.Length)]))
                .ToArray());
    }
}