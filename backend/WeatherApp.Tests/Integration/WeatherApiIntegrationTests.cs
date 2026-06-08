using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WeatherApp.Api.Data;
using WeatherApp.Api.DTOs;
using WeatherApp.Api.Providers;
using Xunit;

namespace WeatherApp.Tests.Integration;

public class WeatherApiIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public WeatherApiIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Replace DB with in-memory for tests
                var dbDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<WeatherDbContext>));
                if (dbDescriptor != null) services.Remove(dbDescriptor);

                services.AddDbContext<WeatherDbContext>(opts =>
                    opts.UseInMemoryDatabase("TestDb_" + Guid.NewGuid()));

                // Always use Fake provider in tests
                var providerDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IWeatherProvider));
                if (providerDescriptor != null) services.Remove(providerDescriptor);
                services.AddSingleton<IWeatherProvider, FakeWeatherProvider>();
            });
        }).CreateClient();
    }

    [Fact]
    public async Task POST_city_ShouldReturn200_WithValidCity()
    {
        var response = await _client.PostAsJsonAsync("/api/weather/city", new { cityName = "Curitiba" });

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<TemperatureResponse>();
        body.Should().NotBeNull();
        body!.CityName.Should().NotBeNullOrEmpty();
        body.Temperature.Should().BeInRange(-20m, 50m);
    }

    [Fact]
    public async Task POST_city_ShouldReturn400_WhenCityNameIsEmpty()
    {
        var response = await _client.PostAsJsonAsync("/api/weather/city", new { cityName = "" });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task POST_coordinates_ShouldReturn200_WithValidCoords()
    {
        var response = await _client.PostAsJsonAsync("/api/weather/coordinates",
            new { latitude = -25.43, longitude = -49.27 });

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<TemperatureResponse>();
        body.Should().NotBeNull();
        body!.Temperature.Should().BeInRange(-20m, 50m);
    }

    [Fact]
    public async Task POST_coordinates_ShouldReturn400_WithInvalidLatitude()
    {
        var response = await _client.PostAsJsonAsync("/api/weather/coordinates",
            new { latitude = 999, longitude = -49.27 });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GET_history_ShouldReturnEmpty_WhenNoCityRegistered()
    {
        var response = await _client.GetAsync("/api/weather/history?city=CidadeInexistente");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<HistoryResponse>();
        body.Should().NotBeNull();
        body!.Records.Should().BeEmpty();
    }

    [Fact]
    public async Task GET_history_ShouldReturn400_WhenNoParamsProvided()
    {
        var response = await _client.GetAsync("/api/weather/history");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GET_history_ShouldReturnRecords_AfterRegistration()
    {
        // First register a temperature
        await _client.PostAsJsonAsync("/api/weather/city", new { cityName = "Porto Alegre" });

        // Then query history
        var response = await _client.GetAsync("/api/weather/history?city=Porto+Alegre");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<HistoryResponse>();
        body!.Records.Should().HaveCountGreaterThan(0);
    }

    [Fact]
    public async Task GET_health_ShouldReturn200()
    {
        var response = await _client.GetAsync("/health");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
