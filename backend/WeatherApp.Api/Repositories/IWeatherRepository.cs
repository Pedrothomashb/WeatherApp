using WeatherApp.Api.Models;

namespace WeatherApp.Api.Repositories;

public interface IWeatherRepository
{
    Task<City?> FindCityByName(string name, CancellationToken ct = default);
    Task<City?> FindCityByCoordinates(decimal latitude, decimal longitude, CancellationToken ct = default);
    Task<City> UpsertCity(string name, decimal? latitude, decimal? longitude, CancellationToken ct = default);
    Task<TemperatureRecord> AddRecord(TemperatureRecord record, CancellationToken ct = default);
    Task<IReadOnlyList<TemperatureRecord>> GetHistory(Guid cityId, int days = 30, CancellationToken ct = default);
}
