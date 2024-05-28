using WireMock.Client.Builders;

namespace MockWeatherForecast.AppHost;
internal class WeatherForecastApiMock
{
    public static async Task Build(AdminApiMappingBuilder builder)
    {
        var summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        builder.Given(builder => builder
            .WithRequest(request => request
                .UsingGet()
                .WithPath("/weatherforecast")
            )
            .WithResponse(response => response
                .WithBodyAsJson(() => Enumerable.Range(1, 5).Select(index =>
                    new WeatherForecast
                    (
                        DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                        Random.Shared.Next(-20, 55),
                        summaries[Random.Shared.Next(summaries.Length)]
                    ))
                    .ToArray())
            )
        );

        await builder.BuildAndPostAsync();
    }
}

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
