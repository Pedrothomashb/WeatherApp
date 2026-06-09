using Microsoft.EntityFrameworkCore;
using Serilog;
using WeatherApp.Api.Data;
using WeatherApp.Api.Middleware;
using WeatherApp.Api.Providers;
using WeatherApp.Api.Repositories;
using WeatherApp.Api.Services;

WebApplicationBuilder objBuilder = WebApplication.CreateBuilder(args);

// Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(objBuilder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

objBuilder.Host.UseSerilog();

// Controllers + Swagger
objBuilder.Services.AddControllers();
objBuilder.Services.AddEndpointsApiExplorer();
objBuilder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "WeatherApp API",
        Version = "v1",
        Description = "API para registro e consulta de temperaturas por cidade ou coordenadas."
    });
    string strFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    string strPath = Path.Combine(AppContext.BaseDirectory, strFile);
    if (File.Exists(strPath))
    {
        c.IncludeXmlComments(strPath);
    }
});

// Database
objBuilder.Services.AddDbContext<WeatherDbContext>(options =>
    options.UseNpgsql(objBuilder.Configuration.GetConnectionString("DefaultConnection")));

// Health Checks
objBuilder.Services.AddHealthChecks()
    .AddDbContextCheck<WeatherDbContext>("database");

// CORS (for Vue frontend)
objBuilder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

// Feature flag: choose provider via config ("OpenWeatherMap" or "Fake")
string strProviderName = objBuilder.Configuration["WeatherProviders:UseProvider"] ?? "Fake";

if (strProviderName == "OpenWeatherMap")
{
    objBuilder.Services.AddHttpClient<IWeatherProvider, OpenWeatherMapProvider>();
}
else
{
    objBuilder.Services.AddSingleton<IWeatherProvider, FakeWeatherProvider>();
}

// App services
objBuilder.Services.AddScoped<IWeatherRepository, WeatherRepository>();
objBuilder.Services.AddScoped<IWeatherService, WeatherService>();

WebApplication objApp = objBuilder.Build();

// Auto-migrate only when using a real relational database (not InMemory used in tests)
using (IServiceScope objScope = objApp.Services.CreateScope())
{
    WeatherDbContext objDb = objScope.ServiceProvider.GetRequiredService<WeatherDbContext>();
    if (objDb.Database.IsRelational())
    {
        await objDb.Database.MigrateAsync();
    }
    else
    {
        await objDb.Database.EnsureCreatedAsync();
    }
}

objApp.UseMiddleware<ExceptionMiddleware>();

objApp.UseCors();

objApp.UseSwagger();
objApp.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "WeatherApp v1"));

objApp.MapControllers();
objApp.MapHealthChecks("/health");

await objApp.RunAsync();

// Needed for integration tests
public partial class Program { }
