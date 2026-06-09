using WeatherApp.Api.DTOs;

namespace WeatherApp.Api.Services;

public interface IWeatherService
{
    Task<TemperatureResponse> RegisterByCity(string pCityName, CancellationToken pCt = default);
    Task<TemperatureResponse> RegisterByCoordinates(decimal pLatitude, decimal pLongitude, CancellationToken pCt = default);
    Task<HistoryResponse> GetHistory(string? pCityName, decimal? pLatitude, decimal? pLongitude, CancellationToken pCt = default);
}
