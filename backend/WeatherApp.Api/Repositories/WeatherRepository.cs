using Microsoft.EntityFrameworkCore;
using WeatherApp.Api.Data;
using WeatherApp.Api.Models;

namespace WeatherApp.Api.Repositories;

public class WeatherRepository : IWeatherRepository
{
    /// <summary>
    /// Contexto do Banco
    /// </summary>
    private readonly WeatherDbContext objDb;

    public WeatherRepository(WeatherDbContext pDb) => objDb = pDb;

    /// <summary>
    /// Procura a cidade pelo nome
    /// </summary>
    /// <param name="pName">Nome da cidade</param>
    /// <param name="pCt">Cancellation Token</param>
    /// <returns>Retorna os dados da cidade do banco pelo nome</returns>
    public Task<City?> FindCityByName(string pName, CancellationToken pCt = default)
        => objDb.Cities.FirstOrDefaultAsync(c => c.Name.ToLower() == pName.ToLower(), pCt);

    /// <summary>
    /// Procura a cidade pelas coordenadas
    /// </summary>
    /// <param name="pLatitude">Latitude</param>
    /// <param name="pLongitude">Longitude</param>
    /// <param name="pCt">Cancellantion Token</param>
    /// <returns>Retorna os dados da cidade do banco pelas coordenadas</returns>
    public Task<City?> FindCityByCoordinates(decimal pLatitude, decimal pLongitude, CancellationToken pCt = default)
    {
        // Match within ~1km tolerance
        const decimal TOLERANCE = 0.01m;
        return objDb.Cities.FirstOrDefaultAsync(c =>
            c.Latitude != null && c.Longitude != null &&
            Math.Abs(c.Latitude.Value - pLatitude) < TOLERANCE &&
            Math.Abs(c.Longitude.Value - pLongitude) < TOLERANCE, pCt);
    }

    /// <summary>
    /// Metodo que insere ou atualiza uma cidade no banco (Update+insert)
    /// </summary>
    /// <param name="pName">Nome da cidade</param>
    /// <param name="pLatitude">Latitude</param>
    /// <param name="pLongitude">Longitude</param>
    /// <param name="pCt">Cancellation Token</param>
    /// <returns>Retorna a cidade que foi onserida ou atualizada</returns>
    public async Task<City> UpsertCity(string pName, decimal? pLatitude, decimal? pLongitude, CancellationToken pCt = default)
    {
        City? objExisting = await FindCityByName(pName, pCt);
        if (objExisting is not null)
        {
            if (pLatitude.HasValue)
            {
                objExisting.Latitude = pLatitude;
            }

            if (pLongitude.HasValue)
            {
                objExisting.Longitude = pLongitude;
            }

            await objDb.SaveChangesAsync(pCt);
            return objExisting;
        }

        City objCity = new City { Name = pName, Latitude = pLatitude, Longitude = pLongitude };
        objDb.Cities.Add(objCity);
        await objDb.SaveChangesAsync(pCt);
        return objCity;
    }

    /// <summary>
    /// Metodo que salva um novo registro de temperatura no banco de dados
    /// </summary>
    /// <param name="pRecord">Registro a ser inserido</param>
    /// <param name="pCt">Cancellation Token</param>
    /// <returns>Retorna o registro de temperatura</returns>
    public async Task<TemperatureRecord> AddRecord(TemperatureRecord pRecord, CancellationToken pCt = default)
    {
        objDb.TemperatureRecords.Add(pRecord);
        await objDb.SaveChangesAsync(pCt);
        return pRecord;
    }

    /// <summary>
    /// Metodo que traz os dados do histórico de resgate de dados
    /// </summary>
    /// <param name="pCityId">ID GUID da cidade</param>
    /// <param name="pDays">Filtro por dias do historico</param>
    /// <param name="pCt">Cancellation Token</param>
    /// <returns>Retorna o historico das cidades requisitadas em um certo periodo de dias</returns>
    public Task<IReadOnlyList<TemperatureRecord>> GetHistory(Guid pCityId, int pDays = 30, CancellationToken pCt = default)
    {
        DateTime dtaSince = DateTime.UtcNow.AddDays(-pDays);
        return objDb.TemperatureRecords
            .Include(r => r.City)
            .Where(r => r.CityId == pCityId && r.RecordedAt >= dtaSince)
            .OrderByDescending(r => r.RecordedAt)
            .ToListAsync(pCt)
            .ContinueWith(t => (IReadOnlyList<TemperatureRecord>)t.Result, pCt);
    }
}
