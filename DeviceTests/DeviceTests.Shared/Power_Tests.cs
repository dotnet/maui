using Xamarin.Essentials;
using Xunit;

namespace DeviceTests
{
    public class Power_Tests
    {
        [Fact]
        public void App_Is_Not_Lower_Power_mode()
        {
            Assert.Equal(EnergySaverStatus.Off, Power.EnergySaverStatus);
        }
    }
}
