using Xunit;
using WeatherForecast.Controllers;
using Microsoft.Extensions.Logging.Abstractions;
using System.Linq;

namespace WeatherForecast.Tests
{
    public class UnitTest1
    {
        [Fact]
        public void Get_ReturnsFiveForecasts()
        {
            var logger = NullLogger<WeatherForecastController>.Instance;
            var controller = new WeatherForecastController(logger);

            var result = controller.Get();

            Assert.NotNull(result);
            Assert.Equal(5, result.Count());
        }
    }
}
