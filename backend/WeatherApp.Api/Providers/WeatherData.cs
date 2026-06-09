namespace WeatherApp.Api.Providers;

public record WeatherData(
    string pCityName,
    decimal pTemperature,
    decimal? pFeelsLike,
    int? pHumidity,
    string? pDescription,
    decimal? pLatitude,
    decimal? pLongitude,
    string pRoviderName
);
