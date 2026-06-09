using FluentAssertions;
using WeatherApp.Api.Providers;
using Xunit;

namespace WeatherApp.Tests.Unit;

public class FakeWeatherProviderTests
{
    private readonly FakeWeatherProvider _provider = new();

    [Theory]
    [InlineData("Curitiba")]
    [InlineData("São Paulo")]
    [InlineData("Manaus")]
    [InlineData("CidadeDesconhecida")]
    public async Task GetByCity_ShouldReturnValidData(string cityName)
    {
        WeatherData objResult = await _provider.GetByCity(cityName);

        objResult.Should().NotBeNull();
        objResult.pTemperature.Should().BeInRange(-20m, 50m);
        objResult.pRoviderName.Should().Be("FakeProvider");
        objResult.pCityName.Should().NotBeNullOrEmpty();
    }

    [Theory]
    [InlineData(-25.43, -49.27)]
    [InlineData(-23.55, -46.63)]
    [InlineData(0.0, 0.0)]
    public async Task GetByCoordinates_ShouldReturnValidData(double lat, double lon)
    {
        WeatherData objResult = await _provider.GetByCoordinates((decimal)lat, (decimal)lon);

        objResult.Should().NotBeNull();
        objResult.pTemperature.Should().BeInRange(-20m, 50m);
        objResult.pRoviderName.Should().Be("FakeProvider");
    }

    [Fact]
    public void Name_ShouldBeFakeProvider()
    {
        _provider.Name.Should().Be("FakeProvider");
    }
}
