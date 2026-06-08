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
        var result = await _provider.GetByCity(cityName);

        result.Should().NotBeNull();
        result.Temperature.Should().BeInRange(-20m, 50m);
        result.ProviderName.Should().Be("FakeProvider");
        result.CityName.Should().NotBeNullOrEmpty();
    }

    [Theory]
    [InlineData(-25.43, -49.27)]
    [InlineData(-23.55, -46.63)]
    [InlineData(0.0, 0.0)]
    public async Task GetByCoordinates_ShouldReturnValidData(double lat, double lon)
    {
        var result = await _provider.GetByCoordinates((decimal)lat, (decimal)lon);

        result.Should().NotBeNull();
        result.Temperature.Should().BeInRange(-20m, 50m);
        result.ProviderName.Should().Be("FakeProvider");
    }

    [Fact]
    public void Name_ShouldBeFakeProvider()
    {
        _provider.Name.Should().Be("FakeProvider");
    }
}
