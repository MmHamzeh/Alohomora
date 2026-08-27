using Alohomora.Core.Attributes;
using Alohomora.Core.Common.Enums;
using Microsoft.AspNetCore.Mvc;

namespace Alohomora.TestApi.Controllers;

[ApiController]
[Route("[controller]")]
[AlohomoraAuth(AuthenticationType.AllowAnonymous)]
public class WeatherForecastController : ControllerBase
{
    [HttpGet(Name = "GetWeatherForecast")]
    //[Alohomora.Attributes.AlohomoraAuth(AuthenticationType.AllowAnonymous)]
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

    [HttpGet(Name = "GetWeatherForecast2")]
    [AlohomoraAuth(AuthenticationType.SignedIn)]
    public IEnumerable<WeatherForecast> Get2()
    {
        return null;
    }

    [HttpGet(Name = "GetWeatherForecast3")]
    [AlohomoraAuth(AuthenticationType.CheckPermissions)]
    public IEnumerable<WeatherForecast> Get3()
    {
        return null;
    }

    [HttpGet(Name = "GetWeatherForecast4")]
    [AlohomoraAuth(roles: ["Admin", "User"])]
    public IEnumerable<WeatherForecast> Get4()
    {
        return null;
    }






    private static readonly string[] Summaries =
    [
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    ];
}
