using System.Text.Json;

namespace WeatherApp.Api.Providers;

public class OpenWeatherMapProvider : IWeatherProvider
{
    /// <summary>
    /// Objeto HTTP do provedor
    /// </summary>
    private readonly HttpClient objHttp;
    /// <summary>
    /// String da API
    /// </summary>
    private readonly string strApiKey;
    /// <summary>
    /// url base da API de fora
    /// </summary>
    private const string BASEURL = "https://api.openweathermap.org/data/2.5";

    /// <summary>
    /// Nome do provedor da API de fora
    /// </summary>
    public string Name => "OpenWeatherMap";

    public OpenWeatherMapProvider(HttpClient pHttp, IConfiguration pConfig)
    {
        objHttp = pHttp;
        strApiKey = pConfig["WeatherProviders:OpenWeatherMap:ApiKey"]
            ?? throw new InvalidOperationException("OpenWeatherMap API key not configured.");
    }

    /// <summary>
    /// Traz os dados da cidade da API OpenWeather
    /// </summary>
    /// <param name="pCityName">Nome da cidade</param>
    /// <param name="pCt">Cancellation Token</param>
    /// <returns>Retorna os dados que vieram da API</returns>
    public async Task<WeatherData> GetByCity(string pCityName, CancellationToken pCt = default)
    {
        string strUrl = $"{BASEURL}/weather?q={Uri.EscapeDataString(pCityName)}&appid={strApiKey}&units=metric&lang=pt_br";
        return await FetchWeather(strUrl, pCt);
    }

    /// <summary>
    /// Traz os dados da cidade pelas coordenadas
    /// </summary>
    /// <param name="pLatitude">Latitude</param>
    /// <param name="pLongitude">Longitude</param>
    /// <param name="pCt">Cancellation Token</param>
    /// <returns>Retorna os dados a partir das coordenadas</returns>
    public async Task<WeatherData> GetByCoordinates(decimal pLatitude, decimal pLongitude, CancellationToken pCt = default)
    {
        string strUrl = $"{BASEURL}/weather?lat={pLatitude}&lon={pLongitude}&appid={strApiKey}&units=metric&lang=pt_br";
        return await FetchWeather(strUrl, pCt);
    }

    /// <summary>
    /// A partir dos dados da URL, faz a requisição para a API da OpenWeather
    /// </summary>
    /// <param name="pUrl">URL com os dados da requisição</param>
    /// <param name="pCt">Cancellation Token</param>
    /// <returns>Dados que são requisitados na url</returns>
    private async Task<WeatherData> FetchWeather(string pUrl, CancellationToken pCt)
    {
        HttpResponseMessage objResponse = await objHttp.GetAsync(pUrl, pCt);
        objResponse.EnsureSuccessStatusCode();

        string strJson = await objResponse.Content.ReadAsStringAsync(pCt);
        using JsonDocument objDoc = JsonDocument.Parse(strJson);
        JsonElement objRoot = objDoc.RootElement;

        string strCityName = objRoot.GetProperty("name").GetString() ?? "Unknown";
        decimal decTemp = objRoot.GetProperty("main").GetProperty("temp").GetDecimal();
        decimal decFeelsLike = objRoot.GetProperty("main").GetProperty("feels_like").GetDecimal();
        int intHumidity = objRoot.GetProperty("main").GetProperty("humidity").GetInt32();
        string? strDescription = objRoot.GetProperty("weather")[0].GetProperty("description").GetString();
        decimal decLat = objRoot.GetProperty("coord").GetProperty("lat").GetDecimal();
        decimal decLon = objRoot.GetProperty("coord").GetProperty("lon").GetDecimal();

        return new WeatherData(strCityName, decTemp, decFeelsLike, intHumidity, strDescription, decLat, decLon, Name);
    }
}
