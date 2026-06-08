namespace WeatherApp.Api.Models;

public class TemperatureRecord
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CityId { get; set; }
    public decimal Temperature { get; set; }
    public decimal? FeelsLike { get; set; }
    public int? Humidity { get; set; }
    public string? Description { get; set; }
    public string? Provider { get; set; }
    public DateTime RecordedAt { get; set; } = DateTime.UtcNow;

    public City City { get; set; } = null!;
}
