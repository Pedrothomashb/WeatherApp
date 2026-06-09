using WeatherApp.Api.Models;

namespace WeatherApp.Api.Repositories;

public interface IWeatherRepository
{
    Task<City?> FindCityByName(string pName, CancellationToken pCt = default);
    Task<City?> FindCityByCoordinates(decimal pLatitude, decimal pLongitude, CancellationToken pCt = default);
    Task<City> UpsertCity(string pName, decimal? pLatitude, decimal? pLongitude, CancellationToken pCt = default);
    Task<TemperatureRecord> AddRecord(TemperatureRecord pRecord, CancellationToken pCt = default);
    Task<IReadOnlyList<TemperatureRecord>> GetHistory(Guid pCityId, int pDays = 30, CancellationToken pCt = default);
}
