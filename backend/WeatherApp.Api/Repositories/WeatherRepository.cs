using Microsoft.EntityFrameworkCore;
using WeatherApp.Api.Data;
using WeatherApp.Api.Models;

namespace WeatherApp.Api.Repositories;

public class WeatherRepository : IWeatherRepository
{
    private readonly WeatherDbContext _db;

    public WeatherRepository(WeatherDbContext db) => _db = db;

    public Task<City?> FindCityByName(string name, CancellationToken ct = default)
        => _db.Cities.FirstOrDefaultAsync(c => c.Name.ToLower() == name.ToLower(), ct);

    public Task<City?> FindCityByCoordinates(decimal latitude, decimal longitude, CancellationToken ct = default)
    {
        // Match within ~1km tolerance
        const decimal tolerance = 0.01m;
        return _db.Cities.FirstOrDefaultAsync(c =>
            c.Latitude != null && c.Longitude != null &&
            Math.Abs(c.Latitude.Value - latitude) < tolerance &&
            Math.Abs(c.Longitude.Value - longitude) < tolerance, ct);
    }

    public async Task<City> UpsertCity(string name, decimal? latitude, decimal? longitude, CancellationToken ct = default)
    {
        var existing = await FindCityByName(name, ct);
        if (existing is not null)
        {
            if (latitude.HasValue) existing.Latitude = latitude;
            if (longitude.HasValue) existing.Longitude = longitude;
            await _db.SaveChangesAsync(ct);
            return existing;
        }

        var city = new City { Name = name, Latitude = latitude, Longitude = longitude };
        _db.Cities.Add(city);
        await _db.SaveChangesAsync(ct);
        return city;
    }

    public async Task<TemperatureRecord> AddRecord(TemperatureRecord record, CancellationToken ct = default)
    {
        _db.TemperatureRecords.Add(record);
        await _db.SaveChangesAsync(ct);
        return record;
    }

    public Task<IReadOnlyList<TemperatureRecord>> GetHistory(Guid cityId, int days = 30, CancellationToken ct = default)
    {
        var since = DateTime.UtcNow.AddDays(-days);
        return _db.TemperatureRecords
            .Include(r => r.City)
            .Where(r => r.CityId == cityId && r.RecordedAt >= since)
            .OrderByDescending(r => r.RecordedAt)
            .ToListAsync(ct)
            .ContinueWith(t => (IReadOnlyList<TemperatureRecord>)t.Result, ct);
    }
}
