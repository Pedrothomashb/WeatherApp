namespace WeatherApp.Api.Models;

public class TemperatureRecord
{
    /// <summary>
    /// GUID com o id das temperaturas
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();
    /// <summary>
    /// GUID com o id da cidade
    /// </summary>
    public Guid CityId { get; set; }
    /// <summary>
    /// Temperatura da cidade
    /// </summary>
    public decimal Temperature { get; set; }
    /// <summary>
    /// Sensação termica
    /// </summary>
    public decimal? FeelsLike { get; set; }
    /// <summary>
    /// Humidade
    /// </summary>
    public int? Humidity { get; set; }
    /// <summary>
    /// Descrição sobre o tempo
    /// </summary>
    public string? Description { get; set; }
    /// <summary>
    /// Proverdor (fake/Api)
    /// </summary>
    public string? Provider { get; set; }
    /// <summary>
    /// Data de cadastro
    /// </summary>
    public DateTime RecordedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Cidade
    /// </summary>
    public City City { get; set; } = null!;
}
