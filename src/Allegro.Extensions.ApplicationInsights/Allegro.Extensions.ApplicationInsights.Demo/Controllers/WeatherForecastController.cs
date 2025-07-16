using System.Net.Http;
using Allegro.Extensions.ApplicationInsights.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Allegro.Extensions.ApplicationInsights.Demo.Controllers;

public record WeatherForecast(DateTime Date, int TemperatureC, string Summary);

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private static readonly string[] Summaries = ["Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"];

    private readonly ILogger<WeatherForecastController> _logger;
    private readonly IHttpClientFactory _httpClientFactory;

    public WeatherForecastController(ILogger<WeatherForecastController> logger, IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
    }

    [HttpGet(Name = "GetWeatherForecast")]
    public async Task<IEnumerable<WeatherForecast>> Get()
    {
        TelemetryContext.Current["TraceIdentifier"] = HttpContext.TraceIdentifier;
        var client = _httpClientFactory.CreateClient();
        await client.GetAsync("https://asd/asd/12331/3321");
        // return Task.FromResult<IEnumerable<WeatherForecast>>(
        //     Enumerable.Range(1, 5).Select(
        //             index => new WeatherForecast(
        //                 DateTime.Now.AddDays(index),
        //                 Random.Shared.Next(-20, 55),
        //                 Summaries[Random.Shared.Next(Summaries.Length)]))
        //         .ToArray());
        return
            Enumerable.Range(1, 5).Select(
                    index => new WeatherForecast(
                        DateTime.Now.AddDays(index),
                        Random.Shared.Next(-20, 55),
                        Summaries[Random.Shared.Next(Summaries.Length)]))
                .ToArray();
    }
}