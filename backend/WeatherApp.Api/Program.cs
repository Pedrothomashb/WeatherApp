using Microsoft.EntityFrameworkCore;
using Serilog;
using WeatherApp.Api.Data;
using WeatherApp.Api.Middleware;
using WeatherApp.Api.Providers;
using WeatherApp.Api.Repositories;
using WeatherApp.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

// Controllers + Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "WeatherApp API",
        Version = "v1",
        Description = "API para registro e consulta de temperaturas por cidade ou coordenadas."
    });
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath)) c.IncludeXmlComments(xmlPath);
});

// Database
builder.Services.AddDbContext<WeatherDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Health Checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<WeatherDbContext>("database");

// CORS (for Vue frontend)
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

// Feature flag: choose provider via config ("OpenWeatherMap" or "Fake")
var providerName = builder.Configuration["WeatherProviders:UseProvider"] ?? "Fake";

if (providerName == "OpenWeatherMap")
{
    builder.Services.AddHttpClient<IWeatherProvider, OpenWeatherMapProvider>();
}
else
{
    builder.Services.AddSingleton<IWeatherProvider, FakeWeatherProvider>();
}

// App services
builder.Services.AddScoped<IWeatherRepository, WeatherRepository>();
builder.Services.AddScoped<IWeatherService, WeatherService>();

var app = builder.Build();

// Auto-migrate only when using a real relational database (not InMemory used in tests)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<WeatherDbContext>();
    if (db.Database.IsRelational())
    {
        db.Database.Migrate();
    }
    else
    {
        db.Database.EnsureCreated();
    }
}

app.UseMiddleware<ExceptionMiddleware>();

app.UseCors();

app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "WeatherApp v1"));

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();

// Needed for integration tests
public partial class Program { }
