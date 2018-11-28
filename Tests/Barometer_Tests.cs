using System;
using Xamarin.Essentials;
using Xunit;

namespace Tests
{
    public class Barometer_Tests
    {
        [Fact]
        public void Barometer_Start() =>
             Assert.Throws<NotImplementedInReferenceAssemblyException>(() => Barometer.Stop());

        [Fact]
        public void Barometer_Stop() =>
            Assert.Throws<NotImplementedInReferenceAssemblyException>(() => Barometer.Start(SensorSpeed.Default));

        [Fact]
        public void Barometer_IsMonitoring() =>
            Assert.False(Barometer.IsMonitoring);

        [Fact]
        public void Barometer_IsSupported() =>
            Assert.Throws<NotImplementedInReferenceAssemblyException>(() => Barometer.IsSupported);

        [Theory]
        [InlineData(0.0, 0.0, true)]
        [InlineData(0.0, 1.0, false)]
        public void BarometerData_Comparison(
            double pressure1,
            double pressure2,
            bool equals)
        {
            var data1 = new BarometerData(pressure1);
            var data2 = new BarometerData(pressure2);

            if (equals)
            {
                Assert.True(data1.Equals(data2));
                Assert.True(data1 == data2);
                Assert.False(data1 != data2);
                Assert.Equal(data1, data2);
                Assert.Equal(data1.GetHashCode(), data2.GetHashCode());
            }
            else
            {
                Assert.False(data1.Equals(data2));
                Assert.False(data1 == data2);
                Assert.True(data1 != data2);
                Assert.NotEqual(data1, data2);
                Assert.NotEqual(data1.GetHashCode(), data2.GetHashCode());
            }
        }
    }
}
