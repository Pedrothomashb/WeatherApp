namespace WeatherApp.Api.DTOs;

public record RegisterByCityRequest(string CityName);

public record RegisterByCoordinatesRequest(decimal Latitude, decimal Longitude);

public record TemperatureResponse(
    Guid Id,
    string CityName,
    decimal Temperature,
    decimal? FeelsLike,
    int? Humidity,
    string? Description,
    string? Provider,
    DateTime RecordedAt
);

public record HistoryRequest(
    string? CityName,
    decimal? Latitude,
    decimal? Longitude
);

public record HistoryResponse(
    string CityName,
    decimal? Latitude,
    decimal? Longitude,
    IEnumerable<TemperatureResponse> Records
);
