namespace WeatherApp.Api.Providers;

/// <summary>
/// Fake provider for demo/testing. Returns realistic simulated data.
/// Enable via feature flag: WeatherProviders:UseProvider = "Fake"
/// </summary>
public class FakeWeatherProvider : IWeatherProvider
{
    /// <summary>
    /// Instância do gerador de números aleatórios do .NET
    /// </summary>
    private static readonly Random objRng = new();
    /// <summary>
    /// Nome do provedor
    /// </summary>
    public string Name => "FakeProvider";

    /// <summary>
    /// Dicionario com os dados fake das cidades
    /// </summary>
    private static readonly Dictionary<string, (decimal Lat, decimal Lon, decimal BaseTemp)> dicCities = new(StringComparer.OrdinalIgnoreCase)
    {
        ["São Paulo"]   = (-23.55m, -46.63m, 22m),
        ["Curitiba"]    = (-25.43m, -49.27m, 18m),
        ["Rio de Janeiro"] = (-22.90m, -43.17m, 28m),
        ["Fortaleza"]   = (-3.72m,  -38.54m, 32m),
        ["Manaus"]      = (-3.10m,  -60.02m, 30m),
        ["Porto Alegre"]= (-30.03m, -51.23m, 20m),
        ["Brasília"]    = (-15.78m, -47.93m, 25m),
        ["Salvador"]    = (-12.97m, -38.50m, 29m),
    };

    /// <summary>
    /// Array de Descrições 
    /// </summary>
    private static readonly string[] strDescriptions =
        ["céu limpo", "poucas nuvens", "nuvens dispersas", "chuva leve", "nublado", "tempestade"];

    /// <summary>
    /// Metodo que busca dados de clima pelo nome da cidade
    /// </summary>
    /// <param name="pCityName">Nome da cidade</param>
    /// <param name="pCt">Cancellation Token</param>
    /// <returns>Retona dados da cidade</returns>
    public Task<WeatherData> GetByCity(string pCityName, CancellationToken pCt = default)
    {
        bool blnKnown = dicCities.TryGetValue(pCityName, out var info);
        decimal decBaseTemp = blnKnown ? info.BaseTemp : 22m;
        decimal decLat = blnKnown ? info.Lat : (decimal)(objRng.NextDouble() * 30 - 15);
        decimal decLon = blnKnown ? info.Lon : (decimal)(objRng.NextDouble() * 70 - 55);

        return Task.FromResult(BuildWeatherData(pCityName, decBaseTemp, decLat, decLon));
    }

    /// <summary>
    /// Retorna a cidade pelas coordenadas
    /// </summary>
    /// <param name="pLatitude">Latitude da cidade</param>
    /// <param name="pLongitude">Longitude da acidade</param>
    /// <param name="pCt">Cancellation Token</param>
    /// <returns>Retorna os dados da cidade</returns>
    public Task<WeatherData> GetByCoordinates(decimal pLatitude, decimal pLongitude, CancellationToken pCt = default)
    {
        KeyValuePair<string, (decimal Lat, decimal Lon, decimal BaseTemp)> objClosestCity = dicCities
            .OrderBy(c => Math.Abs((double)(c.Value.Lat - pLatitude)) + Math.Abs((double)(c.Value.Lon - pLongitude)))
            .First();

        return Task.FromResult(BuildWeatherData(objClosestCity.Key, objClosestCity.Value.BaseTemp, pLatitude, pLongitude));
    }

    /// <summary>
    /// Metodo auxiliar privado que monta o objeto WeatherData com dados simulados.
    /// </summary>
    /// <param name="pCity">Cidade</param>
    /// <param name="pBaseTemp">Temperatura base</param>
    /// <param name="pLat">Latitude</param>
    /// <param name="pLon">Longitude</param>
    /// <returns>Retorna dados montados</returns>
    private WeatherData BuildWeatherData(string pCity, decimal pBaseTemp, decimal pLat, decimal pLon)
    {
        decimal decVariation = (decimal)(objRng.NextDouble() * 6 - 3);
        decimal decTemp = Math.Round(pBaseTemp + decVariation, 1);
        decimal decFeelsLike = Math.Round(decTemp - (decimal)(objRng.NextDouble() * 3), 1);
        int intHumidity = objRng.Next(40, 95);
        string strDesc = strDescriptions[objRng.Next(strDescriptions.Length)];

        return new WeatherData(pCity, decTemp, decFeelsLike, intHumidity, strDesc, pLat, pLon, Name);
    }
}
