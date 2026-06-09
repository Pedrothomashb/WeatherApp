namespace WeatherApp.Api.Providers;

public interface IWeatherProvider
{
    /// <summary>
    /// Nome do provedor
    /// </summary>
    string Name { get; }
    Task<WeatherData> GetByCity(string pCityName, CancellationToken pCt = default);
    Task<WeatherData> GetByCoordinates(decimal pLatitude, decimal pLongitude, CancellationToken pCt = default);
}
