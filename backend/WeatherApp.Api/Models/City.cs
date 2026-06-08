namespace WeatherApp.Api.Models;

public class City
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<TemperatureRecord> TemperatureRecords { get; set; } = new List<TemperatureRecord>();
}
