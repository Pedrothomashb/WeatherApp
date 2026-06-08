namespace WeatherApp.Api.Providers;

public record WeatherData(
    string CityName,
    decimal Temperature,
    decimal? FeelsLike,
    int? Humidity,
    string? Description,
    decimal? Latitude,
    decimal? Longitude,
    string ProviderName
);
