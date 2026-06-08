using Microsoft.AspNetCore.Mvc;
using WeatherApp.Api.DTOs;
using WeatherApp.Api.Services;

namespace WeatherApp.Api.Controllers;

/// <summary>
/// Endpoints para registro e consulta de temperaturas.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class WeatherController : ControllerBase
{
    private readonly IWeatherService _service;
    private readonly ILogger<WeatherController> _logger;

    public WeatherController(IWeatherService service, ILogger<WeatherController> logger)
    {
        _service = service;
        _logger = logger;
    }

    /// <summary>
    /// Registra a temperatura atual de uma cidade pelo nome.
    /// </summary>
    [HttpPost("city")]
    [ProducesResponseType(typeof(TemperatureResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status502BadGateway)]
    public async Task<IActionResult> RegisterByCity(
        [FromBody] RegisterByCityRequest request,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.CityName))
            return BadRequest(new { error = "CityName é obrigatório." });

        var result = await _service.RegisterByCity(request.CityName, ct);
        return Ok(result);
    }

    /// <summary>
    /// Registra a temperatura atual pelas coordenadas geográficas.
    /// </summary>
    [HttpPost("coordinates")]
    [ProducesResponseType(typeof(TemperatureResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RegisterByCoordinates(
        [FromBody] RegisterByCoordinatesRequest request,
        CancellationToken ct)
    {
        if (request.Latitude < -90 || request.Latitude > 90)
            return BadRequest(new { error = "Latitude inválida. Deve estar entre -90 e 90." });

        if (request.Longitude < -180 || request.Longitude > 180)
            return BadRequest(new { error = "Longitude inválida. Deve estar entre -180 e 180." });

        var result = await _service.RegisterByCoordinates(request.Latitude, request.Longitude, ct);
        return Ok(result);
    }

    /// <summary>
    /// Retorna o histórico de temperaturas dos últimos 30 dias para uma cidade ou coordenada.
    /// </summary>
    [HttpGet("history")]
    [ProducesResponseType(typeof(HistoryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetHistory(
        [FromQuery] string? city,
        [FromQuery] decimal? lat,
        [FromQuery] decimal? lon,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(city) && (!lat.HasValue || !lon.HasValue))
            return BadRequest(new { error = "Informe city ou lat+lon." });

        var result = await _service.GetHistory(city, lat, lon, ct);
        return Ok(result);
    }
}
