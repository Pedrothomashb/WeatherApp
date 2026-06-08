using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using WeatherApp.Api.Data;
using WeatherApp.Api.DTOs;
using WeatherApp.Api.Providers;
using Xunit;

namespace WeatherApp.Tests.Integration;

// Shared factory so all tests in this class use the same in-memory database instance
public class WeatherApiFactory : WebApplicationFactory<Program>
{
    private readonly string _dbName = "TestDb_" + Guid.NewGuid();

    protected override void ConfigureWebHost(Microsoft.AspNetCore.Hosting.IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<WeatherDbContext>>();
            services.RemoveAll<WeatherDbContext>();

            services.AddDbContext<WeatherDbContext>(opts =>
                opts.UseInMemoryDatabase(_dbName));

            // Remove DbContext health check (incompatible with InMemory)
            var healthDescriptor = services.SingleOrDefault(d =>
                d.ServiceType.FullName != null &&
                d.ServiceType.FullName.Contains("HealthCheck") &&
                d.ServiceType.FullName.Contains("DbContext"));
            if (healthDescriptor != null) services.Remove(healthDescriptor);

            services.RemoveAll<IWeatherProvider>();
            services.AddSingleton<IWeatherProvider, FakeWeatherProvider>();
        });
    }
}

public class WeatherApiIntegrationTests : IClassFixture<WeatherApiFactory>
{
    private readonly HttpClient _client;

    public WeatherApiIntegrationTests(WeatherApiFactory factory)
    {
        _client = factory.CreateClient();
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
        var response = await _client.GetAsync("/api/weather/history?city=CidadeQueNuncaExistiu");

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
        // Use a unique city name to avoid interference from other tests
        var city = "HistoryTestCity_" + Guid.NewGuid().ToString("N")[..8];

        var post = await _client.PostAsJsonAsync("/api/weather/city", new { cityName = city });
        post.StatusCode.Should().Be(HttpStatusCode.OK);

        var response = await _client.GetAsync($"/api/weather/history?city={Uri.EscapeDataString(city)}");
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
