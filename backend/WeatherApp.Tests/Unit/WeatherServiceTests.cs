using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using WeatherApp.Api.DTOs;
using WeatherApp.Api.Models;
using WeatherApp.Api.Providers;
using WeatherApp.Api.Repositories;
using WeatherApp.Api.Services;
using Xunit;

namespace WeatherApp.Tests.Unit;

public class WeatherServiceTests
{
    private readonly Mock<IWeatherProvider> _providerMock = new();
    private readonly Mock<IWeatherRepository> _repoMock = new();
    private readonly Mock<ILogger<WeatherService>> _loggerMock = new();

    private WeatherService CreateService() =>
        new(_providerMock.Object, _repoMock.Object, _loggerMock.Object);

    [Fact]
    public async Task RegisterByCity_ShouldReturnTemperatureResponse_WhenProviderSucceeds()
    {
        // Arrange
        WeatherData weatherData = new("Curitiba", 18.5m, 16m, 75, "céu limpo", -25.43m, -49.27m, "FakeProvider");
        City city = new() { Id = Guid.NewGuid(), Name = "Curitiba", Latitude = -25.43m, Longitude = -49.27m };
        TemperatureRecord record = new TemperatureRecord
        {
            Id = Guid.NewGuid(), CityId = city.Id, Temperature = 18.5m,
            FeelsLike = 16m, Humidity = 75, Description = "céu limpo",
            Provider = "FakeProvider", RecordedAt = DateTime.UtcNow
        };

        _providerMock.Setup(p => p.GetByCity("Curitiba", default)).ReturnsAsync(weatherData);
        _repoMock.Setup(r => r.UpsertCity("Curitiba", -25.43m, -49.27m, default)).ReturnsAsync(city);
        _repoMock.Setup(r => r.AddRecord(It.IsAny<TemperatureRecord>(), default)).ReturnsAsync(record);

        WeatherService service = CreateService();

        // Act
        TemperatureResponse result = await service.RegisterByCity("Curitiba");

        // Assert
        result.CityName.Should().Be("Curitiba");
        result.Temperature.Should().Be(18.5m);
        result.Humidity.Should().Be(75);
        result.Provider.Should().Be("FakeProvider");
    }

    [Fact]
    public async Task RegisterByCoordinates_ShouldPersistAndReturn_WhenValid()
    {
        // Arrange
        decimal lat = -23.55m;
        decimal lon = -46.63m;
        WeatherData weatherData = new WeatherData("São Paulo", 22m, 20m, 80, "nublado", lat, lon, "FakeProvider");
        City city = new() { Id = Guid.NewGuid(), Name = "São Paulo", Latitude = lat, Longitude = lon };
        TemperatureRecord record = new TemperatureRecord
        {
            Id = Guid.NewGuid(), CityId = city.Id, Temperature = 22m,
            RecordedAt = DateTime.UtcNow, Provider = "FakeProvider"
        };

        _providerMock.Setup(p => p.GetByCoordinates(lat, lon, default)).ReturnsAsync(weatherData);
        _repoMock.Setup(r => r.UpsertCity(It.IsAny<string>(), It.IsAny<decimal?>(), It.IsAny<decimal?>(), default)).ReturnsAsync(city);
        _repoMock.Setup(r => r.AddRecord(It.IsAny<TemperatureRecord>(), default)).ReturnsAsync(record);

        WeatherService service = CreateService();

        // Act
        TemperatureResponse result = await service.RegisterByCoordinates(lat, lon);

        // Assert
        result.Temperature.Should().Be(22m);
        _repoMock.Verify(r => r.AddRecord(It.IsAny<TemperatureRecord>(), default), Times.Once);
    }

    [Fact]
    public async Task GetHistory_ShouldReturnEmpty_WhenCityNotFound()
    {
        // Arrange
        _repoMock.Setup(r => r.FindCityByName("CidadeInexistente", default)).ReturnsAsync((City?)null);
        _repoMock.Setup(r => r.FindCityByCoordinates(It.IsAny<decimal>(), It.IsAny<decimal>(), default)).ReturnsAsync((City?)null);

        var service = CreateService();

        // Act
        var result = await service.GetHistory("CidadeInexistente", null, null);

        // Assert
        result.Records.Should().BeEmpty();
        result.CityName.Should().Be("CidadeInexistente");
    }

    [Fact]
    public async Task GetHistory_ShouldThrow_WhenNeitherCityNorCoordinatesProvided()
    {
        // Arrange
        var service = CreateService();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.GetHistory(null, null, null));
    }

    [Fact]
    public async Task GetHistory_ShouldReturnRecords_WhenCityExists()
    {
        // Arrange
        var cityId = Guid.NewGuid();
        var city = new City { Id = cityId, Name = "Rio de Janeiro" };
        var records = new List<TemperatureRecord>
        {
            new() { Id = Guid.NewGuid(), CityId = cityId, Temperature = 28m, RecordedAt = DateTime.UtcNow, City = city },
            new() { Id = Guid.NewGuid(), CityId = cityId, Temperature = 27m, RecordedAt = DateTime.UtcNow.AddHours(-2), City = city }
        };

        _repoMock.Setup(r => r.FindCityByName("Rio de Janeiro", default)).ReturnsAsync(city);
        _repoMock.Setup(r => r.GetHistory(cityId, 30, default)).ReturnsAsync((IReadOnlyList<TemperatureRecord>)records);

        var service = CreateService();

        // Act
        var result = await service.GetHistory("Rio de Janeiro", null, null);

        // Assert
        result.Records.Should().HaveCount(2);
        result.CityName.Should().Be("Rio de Janeiro");
    }
}
