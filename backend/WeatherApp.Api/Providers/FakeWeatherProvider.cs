namespace WeatherApp.Api.Providers;

/// <summary>
/// Fake provider for demo/testing. Returns realistic simulated data.
/// Enable via feature flag: WeatherProviders:UseProvider = "Fake"
/// </summary>
public class FakeWeatherProvider : IWeatherProvider
{
    private static readonly Random _rng = new();
    public string Name => "FakeProvider";

    private static readonly Dictionary<string, (decimal Lat, decimal Lon, decimal BaseTemp)> _cities = new(StringComparer.OrdinalIgnoreCase)
    {
        ["São Paulo"]   = (-23.55m, -46.63m, 22m),
        ["Curitiba"]    = (-25.43m, -49.27m, 18m),
        ["Rio de Janeiro"] = (-22.90m, -43.17m, 28m),
        ["Fortaleza"]   = (-3.72m,  -38.54m, 32m),
        ["Manaus"]      = (-3.10m,  -60.02m, 30m),
        ["Porto Alegre"]= (-30.03m, -51.23m, 20m),
        ["Brasília"]    = (-15.78m, -47.93m, 25m),
        ["Salvador"]    = (-12.97m, -38.50m, 29m),
    };

    private static readonly string[] _descriptions =
        ["céu limpo", "poucas nuvens", "nuvens dispersas", "chuva leve", "nublado", "tempestade"];

    public Task<WeatherData> GetByCity(string cityName, CancellationToken ct = default)
    {
        var known = _cities.TryGetValue(cityName, out var info);
        var baseTemp = known ? info.BaseTemp : 22m;
        var lat = known ? info.Lat : (decimal)(_rng.NextDouble() * 30 - 15);
        var lon = known ? info.Lon : (decimal)(_rng.NextDouble() * 70 - 55);

        return Task.FromResult(BuildWeatherData(cityName, baseTemp, lat, lon));
    }

    public Task<WeatherData> GetByCoordinates(decimal latitude, decimal longitude, CancellationToken ct = default)
    {
        var closestCity = _cities
            .OrderBy(c => Math.Abs((double)(c.Value.Lat - latitude)) + Math.Abs((double)(c.Value.Lon - longitude)))
            .First();

        return Task.FromResult(BuildWeatherData(closestCity.Key, closestCity.Value.BaseTemp, latitude, longitude));
    }

    private WeatherData BuildWeatherData(string city, decimal baseTemp, decimal lat, decimal lon)
    {
        var variation = (decimal)(_rng.NextDouble() * 6 - 3);
        var temp = Math.Round(baseTemp + variation, 1);
        var feelsLike = Math.Round(temp - (decimal)(_rng.NextDouble() * 3), 1);
        var humidity = _rng.Next(40, 95);
        var desc = _descriptions[_rng.Next(_descriptions.Length)];

        return new WeatherData(city, temp, feelsLike, humidity, desc, lat, lon, Name);
    }
}
