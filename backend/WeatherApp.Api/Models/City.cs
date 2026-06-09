namespace WeatherApp.Api.Models;

public class City
{
    /// <summary>
    /// GUID do id da cidade
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();
    /// <summary>
    /// Nome da cidade
    /// </summary>
    public string Name { get; set; } = string.Empty;
    /// <summary>
    /// Latitude da localização da cidade
    /// </summary>
    public decimal? Latitude { get; set; }
    /// <summary>
    /// Longitude da localização da cidade
    /// </summary>
    public decimal? Longitude { get; set; }
    /// <summary>
    /// Data de criação
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Temperatura
    /// </summary>
    public ICollection<TemperatureRecord> TemperatureRecords { get; set; } = new List<TemperatureRecord>();
}
