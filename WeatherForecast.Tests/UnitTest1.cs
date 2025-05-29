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

        [Fact]
        public void Get_ForecastItemsHaveValidValues()
        {
            var logger = NullLogger<WeatherForecastController>.Instance;
            var controller = new WeatherForecastController(logger);

            var result = controller.Get().ToList();

            Assert.Equal(5, result.Count);
            foreach (var forecast in result)
            {
                Assert.InRange(forecast.TemperatureC, -20, 55);
                Assert.False(string.IsNullOrWhiteSpace(forecast.Summary));
                Assert.InRange<DateTime>(
                    forecast.Date.ToDateTime(TimeOnly.MinValue),
                    DateTime.Now.AddDays(1).Date,
                    DateTime.Now.AddDays(5).Date
                );
            }
        }
    }
}
