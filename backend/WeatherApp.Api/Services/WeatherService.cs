using WeatherApp.Api.DTOs;
using WeatherApp.Api.Models;
using WeatherApp.Api.Providers;
using WeatherApp.Api.Repositories;

namespace WeatherApp.Api.Services;

public class WeatherService : IWeatherService
{
    private readonly IWeatherProvider _provider;
    private readonly IWeatherRepository _repository;
    private readonly ILogger<WeatherService> _logger;

    public WeatherService(IWeatherProvider provider, IWeatherRepository repository, ILogger<WeatherService> logger)
    {
        _provider = provider;
        _repository = repository;
        _logger = logger;
    }

    public async Task<TemperatureResponse> RegisterByCity(string cityName, CancellationToken ct = default)
    {
        _logger.LogInformation("Registering temperature for city: {CityName}", cityName);

        var data = await _provider.GetByCity(cityName, ct);
        var city = await _repository.UpsertCity(data.pCityName, data.pLatitude, data.pLongitude, ct);
        var record = await SaveRecord(city.Id, data, ct);

        return ToResponse(record, data.pCityName);
    }

    public async Task<TemperatureResponse> RegisterByCoordinates(decimal latitude, decimal longitude, CancellationToken ct = default)
    {
        _logger.LogInformation("Registering temperature for coordinates: {Lat}, {Lon}", latitude, longitude);

        var data = await _provider.GetByCoordinates(latitude, longitude, ct);
        var city = await _repository.UpsertCity(data.pCityName, data.pLatitude ?? latitude, data.pLongitude ?? longitude, ct);
        var record = await SaveRecord(city.Id, data, ct);

        return ToResponse(record, data.pCityName);
    }

    public async Task<HistoryResponse> GetHistory(string? cityName, decimal? latitude, decimal? longitude, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(cityName) && (!latitude.HasValue || !longitude.HasValue))
            throw new ArgumentException("Either city name or coordinates must be provided.");

        Models.City? city = null;

        if (!string.IsNullOrWhiteSpace(cityName))
            city = await _repository.FindCityByName(cityName, ct);

        if (city is null && latitude.HasValue && longitude.HasValue)
            city = await _repository.FindCityByCoordinates(latitude.Value, longitude.Value, ct);

        if (city is null)
        {
            var name = cityName ?? $"{latitude},{longitude}";
            return new HistoryResponse(name, latitude, longitude, Enumerable.Empty<TemperatureResponse>());
        }

        var records = await _repository.GetHistory(city.Id, 30, ct);
        var responses = records.Select(r => ToResponse(r, city.Name));

        return new HistoryResponse(city.Name, city.Latitude, city.Longitude, responses);
    }

    private Task<TemperatureRecord> SaveRecord(Guid cityId, WeatherData data, CancellationToken ct)
    {
        var record = new TemperatureRecord
        {
            CityId = cityId,
            Temperature = data.pTemperature,
            FeelsLike = data.pFeelsLike,
            Humidity = data.pHumidity,
            Description = data.pDescription,
            Provider = data.pRoviderName,
            RecordedAt = DateTime.UtcNow
        };
        return _repository.AddRecord(record, ct);
    }

    private static TemperatureResponse ToResponse(TemperatureRecord r, string cityName) =>
        new(r.Id, cityName, r.Temperature, r.FeelsLike, r.Humidity, r.Description, r.Provider, r.RecordedAt);
}
