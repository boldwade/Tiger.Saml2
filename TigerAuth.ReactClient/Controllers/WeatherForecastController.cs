using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using TigerAuth.Client.Web;

namespace TigerAuth.ReactClient.Controllers
{
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
        }

        [HttpGet]
        public IEnumerable<WeatherForecast> Get()
        {
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
        }

        [HttpGet("WhoAmI")]
        public async Task<dynamic> WhoAmIAsync([FromServices] Authenticator authenticator)
        {
            Debug.WriteLine("WhoAmI ");

            if (!(HttpContext.User.Identity?.IsAuthenticated ?? false))
            {
                var result = await authenticator.AuthenticateAsync(HttpContext).ConfigureAwait(false);
                return Ok(new { redirect = result?.Redirect });
            }

            return Ok();
        }
    }
}