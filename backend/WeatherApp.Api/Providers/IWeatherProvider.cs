namespace WeatherApp.Api.Providers;

public interface IWeatherProvider
{
    string Name { get; }
    Task<WeatherData> GetByCity(string cityName, CancellationToken ct = default);
    Task<WeatherData> GetByCoordinates(decimal latitude, decimal longitude, CancellationToken ct = default);
}
