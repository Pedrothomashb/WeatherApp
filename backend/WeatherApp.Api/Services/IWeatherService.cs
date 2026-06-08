using WeatherApp.Api.DTOs;

namespace WeatherApp.Api.Services;

public interface IWeatherService
{
    Task<TemperatureResponse> RegisterByCity(string cityName, CancellationToken ct = default);
    Task<TemperatureResponse> RegisterByCoordinates(decimal latitude, decimal longitude, CancellationToken ct = default);
    Task<HistoryResponse> GetHistory(string? cityName, decimal? latitude, decimal? longitude, CancellationToken ct = default);
}
