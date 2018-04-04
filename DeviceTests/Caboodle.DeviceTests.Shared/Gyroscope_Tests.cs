using System.Threading.Tasks;
using Microsoft.Caboodle;
using Xunit;

namespace Caboodle.DeviceTests
{
    public class Gyroscope_Tests
    {
        bool TestSupported =>
            (DeviceInfo.DeviceType == DeviceType.Physical && DeviceInfo.Platform == DeviceInfo.Platforms.Android) ||
            (DeviceInfo.DeviceType == DeviceType.Physical && DeviceInfo.Platform == DeviceInfo.Platforms.iOS);

        public Gyroscope_Tests()
        {
            Gyroscope.Stop();
        }

        [Fact]
        public void IsSupported()
        {
            if (!TestSupported)
            {
                Assert.False(Gyroscope.IsSupported);
                return;
            }

            Assert.True(Gyroscope.IsSupported);
        }

        [Theory]
        [InlineData(SensorSpeed.Fastest)]
        public async Task Monitor(SensorSpeed sensorSpeed)
        {
            if (!TestSupported)
            {
                return;
            }

            var tcs = new TaskCompletionSource<GyroscopeData>();
            Gyroscope.ReadingChanged += Gyroscope_ReadingChanged;
            Gyroscope.Start(sensorSpeed);

            void Gyroscope_ReadingChanged(GyroscopeChangedEventArgs e)
            {
                tcs.TrySetResult(e.Reading);
            }

            var d = await tcs.Task;

            Gyroscope.Stop();
            Gyroscope.ReadingChanged -= Gyroscope_ReadingChanged;
        }

        [Theory]
        [InlineData(SensorSpeed.Fastest)]
        public async Task IsMonitoring(SensorSpeed sensorSpeed)
        {
            if (!TestSupported)
            {
                return;
            }

            var tcs = new TaskCompletionSource<GyroscopeData>();
            Gyroscope.ReadingChanged += Gyroscope_ReadingChanged;
            Gyroscope.Start(sensorSpeed);

            void Gyroscope_ReadingChanged(GyroscopeChangedEventArgs e)
            {
                tcs.TrySetResult(e.Reading);
            }

            var d = await tcs.Task;
            Assert.True(Gyroscope.IsMonitoring);
            Gyroscope.Stop();
            Gyroscope.ReadingChanged -= Gyroscope_ReadingChanged;
        }

        [Theory]
        [InlineData(SensorSpeed.Fastest)]
        public async Task Stop_Monitor(SensorSpeed sensorSpeed)
        {
            if (!TestSupported)
            {
                return;
            }

            var tcs = new TaskCompletionSource<GyroscopeData>();

            Gyroscope.ReadingChanged += Gyroscope_ReadingChanged;
            Gyroscope.Start(sensorSpeed);

            void Gyroscope_ReadingChanged(GyroscopeChangedEventArgs e)
            {
                tcs.TrySetResult(e.Reading);
            }

            var d = await tcs.Task;

            Gyroscope.Stop();
            Gyroscope.ReadingChanged -= Gyroscope_ReadingChanged;

            Assert.False(Gyroscope.IsMonitoring);
        }
    }
}
