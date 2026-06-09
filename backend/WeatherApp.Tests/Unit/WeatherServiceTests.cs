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
        WeatherData objWeatherData = new("Curitiba", 18.5m, 16m, 75, "céu limpo", -25.43m, -49.27m, "FakeProvider");
        City objCity = new() { Id = Guid.NewGuid(), Name = "Curitiba", Latitude = -25.43m, Longitude = -49.27m };
        TemperatureRecord objRecord = new TemperatureRecord
        {
            Id = Guid.NewGuid(), CityId = objCity.Id, Temperature = 18.5m,
            FeelsLike = 16m, Humidity = 75, Description = "céu limpo",
            Provider = "FakeProvider", RecordedAt = DateTime.UtcNow
        };

        _providerMock.Setup(p => p.GetByCity("Curitiba", default)).ReturnsAsync(objWeatherData);
        _repoMock.Setup(r => r.UpsertCity("Curitiba", -25.43m, -49.27m, default)).ReturnsAsync(objCity);
        _repoMock.Setup(r => r.AddRecord(It.IsAny<TemperatureRecord>(), default)).ReturnsAsync(objRecord);

        WeatherService objService = CreateService();

        // Act
        TemperatureResponse objResult = await objService.RegisterByCity("Curitiba");

        // Assert
        objResult.pCityName.Should().Be("Curitiba");
        objResult.pTemperature.Should().Be(18.5m);
        objResult.pHumidity.Should().Be(75);
        objResult.pRovider.Should().Be("FakeProvider");
    }

    [Fact]
    public async Task RegisterByCoordinates_ShouldPersistAndReturn_WhenValid()
    {
        // Arrange
        decimal decLat = -23.55m;
        decimal decLon = -46.63m;
        WeatherData objWeatherData = new WeatherData("São Paulo", 22m, 20m, 80, "nublado", decLat, decLon, "FakeProvider");
        City objCity = new() { Id = Guid.NewGuid(), Name = "São Paulo", Latitude = decLat, Longitude = decLon };
        TemperatureRecord objRecord = new TemperatureRecord
        {
            Id = Guid.NewGuid(), CityId = objCity.Id, Temperature = 22m,
            RecordedAt = DateTime.UtcNow, Provider = "FakeProvider"
        };

        _providerMock.Setup(p => p.GetByCoordinates(decLat, decLon, default)).ReturnsAsync(objWeatherData);
        _repoMock.Setup(r => r.UpsertCity(It.IsAny<string>(), It.IsAny<decimal?>(), It.IsAny<decimal?>(), default)).ReturnsAsync(objCity);
        _repoMock.Setup(r => r.AddRecord(It.IsAny<TemperatureRecord>(), default)).ReturnsAsync(objRecord);

        WeatherService objService = CreateService();

        // Act
        TemperatureResponse objResult = await objService.RegisterByCoordinates(decLat, decLon);

        // Assert
        objResult.pTemperature.Should().Be(22m);
        _repoMock.Verify(r => r.AddRecord(It.IsAny<TemperatureRecord>(), default), Times.Once);
    }

    [Fact]
    public async Task GetHistory_ShouldReturnEmpty_WhenCityNotFound()
    {
        // Arrange
        _repoMock.Setup(r => r.FindCityByName("CidadeInexistente", default)).ReturnsAsync((City?)null);
        _repoMock.Setup(r => r.FindCityByCoordinates(It.IsAny<decimal>(), It.IsAny<decimal>(), default)).ReturnsAsync((City?)null);

        WeatherService objService = CreateService();

        // Act
        HistoryResponse objResult = await objService.GetHistory("CidadeInexistente", null, null);

        // Assert
        objResult.pRecords.Should().BeEmpty();
        objResult.pCityName.Should().Be("CidadeInexistente");
    }

    [Fact]
    public async Task GetHistory_ShouldThrow_WhenNeitherCityNorCoordinatesProvided()
    {
        // Arrange
        WeatherService objService = CreateService();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            objService.GetHistory(null, null, null));
    }

    [Fact]
    public async Task GetHistory_ShouldReturnRecords_WhenCityExists()
    {
        // Arrange
        Guid objCityId = Guid.NewGuid();
        City objCity = new City { Id = objCityId, Name = "Rio de Janeiro" };
        var lstRecords = new List<TemperatureRecord>
        {
            new() { Id = Guid.NewGuid(), CityId = objCityId, Temperature = 28m, RecordedAt = DateTime.UtcNow, City = objCity },
            new() { Id = Guid.NewGuid(), CityId = objCityId, Temperature = 27m, RecordedAt = DateTime.UtcNow.AddHours(-2), City = objCity }
        };

        _repoMock.Setup(r => r.FindCityByName("Rio de Janeiro", default)).ReturnsAsync(objCity);
        _repoMock.Setup(r => r.GetHistory(objCityId, 30, default)).ReturnsAsync((IReadOnlyList<TemperatureRecord>)lstRecords);

        WeatherService objService = CreateService();

        // Act
        HistoryResponse objResult = await objService.GetHistory("Rio de Janeiro", null, null);

        // Assert
        objResult.pRecords.Should().HaveCount(2);
        objResult.pCityName.Should().Be("Rio de Janeiro");
    }
}
