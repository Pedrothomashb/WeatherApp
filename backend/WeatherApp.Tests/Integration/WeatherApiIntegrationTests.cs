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
    private readonly string strDbName = "TestDb_" + Guid.NewGuid();

    protected override void ConfigureWebHost(Microsoft.AspNetCore.Hosting.IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<WeatherDbContext>>();
            services.RemoveAll<WeatherDbContext>();

            services.AddDbContext<WeatherDbContext>(opts =>
                opts.UseInMemoryDatabase(strDbName));

            // Remove DbContext health check (incompatible with InMemory)
            ServiceDescriptor? objHealthDescriptor = services.SingleOrDefault(d =>
                d.ServiceType.FullName != null &&
                d.ServiceType.FullName.Contains("HealthCheck") &&
                d.ServiceType.FullName.Contains("DbContext"));
            if (objHealthDescriptor != null)
            {
                services.Remove(objHealthDescriptor);
            }

            services.RemoveAll<IWeatherProvider>();
            services.AddSingleton<IWeatherProvider, FakeWeatherProvider>();
        });
    }
}

public class WeatherApiIntegrationTests : IClassFixture<WeatherApiFactory>
{
    private readonly HttpClient objClient;

    public WeatherApiIntegrationTests(WeatherApiFactory pFactory)
    {
        objClient = pFactory.CreateClient();
    }

    [Fact]
    public async Task POST_city_ShouldReturn200_WithValidCity()
    {
        HttpResponseMessage objResponse = await objClient.PostAsJsonAsync("/api/weather/city", new { cityName = "Curitiba" });

        objResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        TemperatureResponse? objBody = await objResponse.Content.ReadFromJsonAsync<TemperatureResponse>();
        objBody.Should().NotBeNull();
        objBody!.pCityName.Should().NotBeNullOrEmpty();
        objBody.pTemperature.Should().BeInRange(-20m, 50m);
    }

    [Fact]
    public async Task POST_city_ShouldReturn400_WhenCityNameIsEmpty()
    {
        HttpResponseMessage objResponse = await objClient.PostAsJsonAsync("/api/weather/city", new { cityName = "" });

        objResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task POST_coordinates_ShouldReturn200_WithValidCoords()
    {
        HttpResponseMessage objResponse = await objClient.PostAsJsonAsync("/api/weather/coordinates",
            new { latitude = -25.43, longitude = -49.27 });

        objResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        TemperatureResponse? objBody = await objResponse.Content.ReadFromJsonAsync<TemperatureResponse>();
        objBody.Should().NotBeNull();
        objBody!.pTemperature.Should().BeInRange(-20m, 50m);
    }

    [Fact]
    public async Task POST_coordinates_ShouldReturn400_WithInvalidLatitude()
    {
        HttpResponseMessage objResponse = await objClient.PostAsJsonAsync("/api/weather/coordinates",
            new { latitude = 999, longitude = -49.27 });

        objResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GET_history_ShouldReturnEmpty_WhenNoCityRegistered()
    {
        HttpResponseMessage objResponse = await objClient.GetAsync("/api/weather/history?city=CidadeQueNuncaExistiu");

        objResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        HistoryResponse? objBody = await objResponse.Content.ReadFromJsonAsync<HistoryResponse>();
        objBody.Should().NotBeNull();
        objBody!.pRecords.Should().BeEmpty();
    }

    [Fact]
    public async Task GET_history_ShouldReturn400_WhenNoParamsProvided()
    {
        HttpResponseMessage objResponse = await objClient.GetAsync("/api/weather/history");

        objResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GET_history_ShouldReturnRecords_AfterRegistration()
    {
        // Use a unique city name to avoid interference from other tests
        string strCity = "HistoryTestCity_" + Guid.NewGuid().ToString("N")[..8];

        HttpResponseMessage objPost = await objClient.PostAsJsonAsync("/api/weather/city", new { cityName = strCity });
        objPost.StatusCode.Should().Be(HttpStatusCode.OK);

        HttpResponseMessage objResponse = await objClient.GetAsync($"/api/weather/history?city={Uri.EscapeDataString(strCity)}");
        objResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        HistoryResponse? objBody = await objResponse.Content.ReadFromJsonAsync<HistoryResponse>();
        objBody!.pRecords.Should().HaveCountGreaterThan(0);
    }

    [Fact]
    public async Task GET_health_ShouldReturn200()
    {
        HttpResponseMessage objResponse = await objClient.GetAsync("/health");
        objResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
