using System.Text.Json;

namespace WeatherApp.Api.Providers;

public class OpenWeatherMapProvider : IWeatherProvider
{
    private readonly HttpClient _http;
    private readonly string _apiKey;
    private const string BaseUrl = "https://api.openweathermap.org/data/2.5";

    public string Name => "OpenWeatherMap";

    public OpenWeatherMapProvider(HttpClient http, IConfiguration config)
    {
        _http = http;
        _apiKey = config["WeatherProviders:OpenWeatherMap:ApiKey"]
            ?? throw new InvalidOperationException("OpenWeatherMap API key not configured.");
    }

    public async Task<WeatherData> GetByCity(string cityName, CancellationToken ct = default)
    {
        var url = $"{BaseUrl}/weather?q={Uri.EscapeDataString(cityName)}&appid={_apiKey}&units=metric&lang=pt_br";
        return await FetchWeather(url, ct);
    }

    public async Task<WeatherData> GetByCoordinates(decimal latitude, decimal longitude, CancellationToken ct = default)
    {
        var url = $"{BaseUrl}/weather?lat={latitude}&lon={longitude}&appid={_apiKey}&units=metric&lang=pt_br";
        return await FetchWeather(url, ct);
    }

    private async Task<WeatherData> FetchWeather(string url, CancellationToken ct)
    {
        var response = await _http.GetAsync(url, ct);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(ct);
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        var cityName = root.GetProperty("name").GetString() ?? "Unknown";
        var temp = root.GetProperty("main").GetProperty("temp").GetDecimal();
        var feelsLike = root.GetProperty("main").GetProperty("feels_like").GetDecimal();
        var humidity = root.GetProperty("main").GetProperty("humidity").GetInt32();
        var description = root.GetProperty("weather")[0].GetProperty("description").GetString();
        var lat = root.GetProperty("coord").GetProperty("lat").GetDecimal();
        var lon = root.GetProperty("coord").GetProperty("lon").GetDecimal();

        return new WeatherData(cityName, temp, feelsLike, humidity, description, lat, lon, Name);
    }
}
