using System;
using Xamarin.Essentials;
using Xunit;

namespace Tests
{
    public class Barometer_Tests
    {
        [Fact]
        public void IsSupported_On_NetStandard() =>
            Assert.Throws<NotImplementedInReferenceAssemblyException>(() => Barometer.IsSupported);

        [Fact]
        public void Monitor_On_NetStandard() =>
            Assert.Throws<NotImplementedInReferenceAssemblyException>(() => Barometer.Start(SensorSpeed.UI));

        [Fact]
        public void Monitor_Off_NetStandard() =>
            Assert.Throws<NotImplementedInReferenceAssemblyException>(() => Barometer.Stop());

        [Fact]
        public void IsMonitoring_Default_On_NetStandard() =>
            Assert.False(Barometer.IsMonitoring);

        [Fact]
        public void BarometerData_Comparison_Equal()
        {
            var device1 = new BarometerData(0);
            var device2 = new BarometerData(0);

            Assert.True(device1.Equals(device2));
            Assert.True(device1 == device2);
            Assert.False(device1 != device2);
            Assert.Equal(device1, device2);
            Assert.Equal(device1.GetHashCode(), device2.GetHashCode());
        }

        [Fact]
        public void BarometerData_Comparison_NotEqual()
        {
            var device1 = new BarometerData(0);
            var device2 = new BarometerData(1);

            Assert.False(device1.Equals(device2));
            Assert.False(device1 == device2);
            Assert.True(device1 != device2);
            Assert.NotEqual(device1, device2);
            Assert.NotEqual(device1.GetHashCode(), device2.GetHashCode());
        }
    }
}
