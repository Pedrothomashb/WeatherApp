namespace WeatherApp.Api.DTOs;

public record RegisterByCityRequest(string CityName);

public record RegisterByCoordinatesRequest(decimal Latitude, decimal Longitude);

public record TemperatureResponse(
    Guid pId,
    string pCityName,
    decimal pTemperature,
    decimal? pFeelsLike,
    int? pHumidity,
    string? pDescription,
    string? pRovider,
    DateTime pRecordedAt
);

public record HistoryRequest(
    string? pCityName,
    decimal? pLatitude,
    decimal? pLongitude
);

public record HistoryResponse(
    string? pCityName,
    decimal? pLatitude,
    decimal? pLongitude,
    IEnumerable<TemperatureResponse> pRecords
);
