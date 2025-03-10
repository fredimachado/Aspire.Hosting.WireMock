using MockWeatherForecast.AppHost;

var builder = DistributedApplication.CreateBuilder(args);

var apiService = builder.AddWireMockNet("apiservice")
    .WithApiMappingBuilder(WeatherForecastApiMock.Build);

builder.AddProject<Projects.MockWeatherForecast_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithReference(apiService)
    .WaitFor(apiService);

builder.Build().Run();
