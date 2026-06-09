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
    /// <summary>
    /// Objeto de Serviço
    /// </summary>
    private readonly IWeatherService objService;
    /// <summary>
    /// Logger da classe Weather
    /// </summary>
    private readonly ILogger<WeatherController> objLogger;

    public WeatherController(IWeatherService pService, ILogger<WeatherController> logger)
    {
        objService = pService;
        objLogger = logger;
    }

    /// <summary>
    /// Registra a temperatura atual de uma cidade pelo nome.
    /// </summary>
    /// <param name="pRequest">Request da requisição</param>
    /// <param name="pCt">Cancellation Token</param>
    /// <returns>Retorna se consegiu registrar a cidade ou não</returns>
    [HttpPost("city")]
    [ProducesResponseType(typeof(TemperatureResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status502BadGateway)]
    public async Task<IActionResult> RegisterByCity(
        [FromBody] RegisterByCityRequest pRequest,
        CancellationToken pCt)
    {
        if (string.IsNullOrWhiteSpace(pRequest.CityName))
        {
            return BadRequest(new { error = "CityName é obrigatório." });
        }

        TemperatureResponse objResult = await objService.RegisterByCity(pRequest.CityName, pCt);
        return Ok(objResult);
    }

    /// <summary>
    /// Registra a temperatura atual pelas coordenadas geográficas.
    /// </summary>
    /// <param name="pRequest">Request da requisição</param>
    /// <param name="pCt">Cancellation Token</param>
    /// <returns>Retorna se conseguiu registrar a temperatura atual</returns>
    [HttpPost("coordinates")]
    [ProducesResponseType(typeof(TemperatureResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RegisterByCoordinates(
        [FromBody] RegisterByCoordinatesRequest pRequest,
        CancellationToken pCt)
    {
        if (pRequest.Latitude < -90 || pRequest.Latitude > 90)
        {
            return BadRequest(new { error = "Latitude inválida. Deve estar entre -90 e 90." });
        }

        if (pRequest.Longitude < -180 || pRequest.Longitude > 180)
        {
            return BadRequest(new { error = "Longitude inválida. Deve estar entre -180 e 180." });
        }

        TemperatureResponse objResult = await objService.RegisterByCoordinates(pRequest.Latitude, pRequest.Longitude, pCt);
        return Ok(objResult);
    }

    /// <summary>
    /// Retorna o histórico de temperaturas dos últimos 30 dias para uma cidade ou coordenada.
    /// </summary>
    /// <param name="pLon">Longitude</param>
    /// <param name="pLat">Latitude</param>
    /// <param name="pCity">Cidade</param>
    /// <param name="pCt">Cancellation Token</param>
    /// <returns>Retorna o historico de cidades pesquisadas</returns>
    [HttpGet("history")]
    [ProducesResponseType(typeof(HistoryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetHistory(
    [FromQuery(Name = "city")] string? pCity,
    [FromQuery(Name = "lat")] decimal? pLat,
    [FromQuery(Name = "lon")] decimal? pLon,
    CancellationToken pCt)
    {
        if (string.IsNullOrWhiteSpace(pCity) && (!pLat.HasValue || !pLon.HasValue))
        {
            return BadRequest(new { error = "Informe city ou lat+lon." });
        }

        HistoryResponse objResult = await objService.GetHistory(pCity, pLat, pLon, pCt);
        return Ok(objResult);
    }
}
