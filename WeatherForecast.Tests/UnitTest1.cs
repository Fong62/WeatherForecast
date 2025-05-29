using Xunit;

namespace WeatherForecast.Tests
{
    public class FailingTest
    {
        [Fact]
        public void ThisTestWillFail()
        {
            int expected = 10;
            int actual = 5 + 2;

            Assert.Equal(expected, actual);
        }
    }
}
