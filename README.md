# Check the official WireMock Aspire package

https://github.com/wiremock/WireMock.Net/wiki/Using-WireMock.Net.Aspire

# Aspire.Hosting.WireMock ![CI](https://github.com/fredimachado/Aspire.Hosting.WireMock/actions/workflows/ci.yml/badge.svg)

Aspire.Hosting.WireMock adds API mocking support for [.NET Aspire](https://github.com/dotnet/aspire) using WireMock.Net.

This project was created to make it easier to mock external APIs that are out of our control.

## Installing the Aspire.Hosting.WireMock NuGet package

[![NuGet](https://img.shields.io/nuget/v/Fredi.Aspire.Hosting.WireMock.svg)](https://www.nuget.org/packages/Fredi.Aspire.Hosting.WireMock)

Install the `Fredi.Aspire.Hosting.WireMock` package into your Aspire App Host project, e.g. using the `dotnet` command line in the project directory:

```shell
dotnet add package Fredi.Aspire.Hosting.WireMock
```

Note: I couldn't push `Aspire.Hosting.WireMock` to NuGet because the `Aspire` package ID prefix is reserved.

## How to use

Please check the [sample project](./samples/MockWeatherForecast).

## Open WireMock Inspector Command

![image](https://github.com/user-attachments/assets/5bd38e80-7d47-4393-bcfc-1c2c9eee48c8)

**Note:** Requires installation of the [WireMockInspector](https://github.com/WireMock-Net/WireMockInspector) tool.
```
dotnet tool install WireMockInspector --global --no-cache --ignore-failed-sources
```

## License

Aspire.Hosting.WireMock is licensed under the [MIT License](./LICENSE).

## Contributing

Feel free to contribute to Aspire.Hosting.WireMock. Please [log an issue](https://github.com/fredimachado/Aspire.Hosting.WireMock/issues/new) to discuss your contribution before submitting a pull request.
