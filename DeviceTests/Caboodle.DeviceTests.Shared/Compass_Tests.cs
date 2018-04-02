using System;
using System.Threading.Tasks;
using Microsoft.Caboodle;
using Xunit;

namespace Caboodle.DeviceTests
{
    public class Compass_Tests
    {
        public Compass_Tests()
        {
            Compass.Stop();
        }

        [Fact]
        public void IsSupported()
        {
            if (DeviceInfo.DeviceType == DeviceType.Virtual && DeviceInfo.Platform == DeviceInfo.Platforms.iOS)
            {
                Assert.False(Compass.IsSupported);
                return;
            }

            Assert.True(Compass.IsSupported);
        }

        [Theory]
        [InlineData(SensorSpeed.Fastest)]
        public async Task Monitor(SensorSpeed sensorSpeed)
        {
            if (DeviceInfo.DeviceType == DeviceType.Virtual && DeviceInfo.Platform == DeviceInfo.Platforms.iOS)
            {
                return;
            }

            var tcs = new TaskCompletionSource<CompassData>();

            Compass.ReadingChanged += Compass_ReadingChanged;
            void Compass_ReadingChanged(CompassChangedEventArgs e)
            {
                tcs.TrySetResult(e.Reading);
            }
            Compass.Start(sensorSpeed);

            var d = await tcs.Task;

            Assert.True(d.HeadingMagneticNorth >= 0);
            Compass.Stop();
            Compass.ReadingChanged -= Compass_ReadingChanged;
        }

        [Theory]
        [InlineData(SensorSpeed.Fastest)]
        public async Task IsMonitoring(SensorSpeed sensorSpeed)
        {
            if (DeviceInfo.DeviceType == DeviceType.Virtual && DeviceInfo.Platform == DeviceInfo.Platforms.iOS)
            {
                return;
            }

            var tcs = new TaskCompletionSource<CompassData>();
            Compass.ReadingChanged += Compass_ReadingChanged;
            void Compass_ReadingChanged(CompassChangedEventArgs e)
            {
                tcs.TrySetResult(e.Reading);
            }
            Compass.Start(sensorSpeed);

            var d = await tcs.Task;
            Assert.True(Compass.IsMonitoring);

            Compass.Stop();
            Compass.ReadingChanged -= Compass_ReadingChanged;
        }

        [Theory]
        [InlineData(SensorSpeed.Fastest)]
        public async Task Stop_Monitor(SensorSpeed sensorSpeed)
        {
            if (DeviceInfo.DeviceType == DeviceType.Virtual && DeviceInfo.Platform == DeviceInfo.Platforms.iOS)
            {
                return;
            }

            var tcs = new TaskCompletionSource<CompassData>();
            Compass.ReadingChanged += Compass_ReadingChanged;
            void Compass_ReadingChanged(CompassChangedEventArgs e)
            {
                tcs.TrySetResult(e.Reading);
            }
            Compass.Start(sensorSpeed);

            var d = await tcs.Task;

            Compass.Stop();
            Compass.ReadingChanged -= Compass_ReadingChanged;

            Assert.False(Compass.IsMonitoring);
        }
    }
}
