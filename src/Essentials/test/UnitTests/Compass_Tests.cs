using System;
using Xamarin.Essentials;
using Xunit;

namespace Tests
{
    public class Compass_Tests
    {
        [Fact]
        public void Compass_Start() =>
            Assert.Throws<NotImplementedInReferenceAssemblyException>(() => Compass.Stop());

        [Fact]
        public void Compass_Stop() =>
            Assert.Throws<NotImplementedInReferenceAssemblyException>(() => Compass.Start(SensorSpeed.Default));

        [Fact]
        public void Compass_IsMonitoring() =>
            Assert.False(Compass.IsMonitoring);

        [Fact]
        public void Compass_IsSupported() =>
                  Assert.Throws<NotImplementedInReferenceAssemblyException>(() => Compass.IsSupported);

        [Theory]
        [InlineData(0.0, 0.0, true)]
        [InlineData(0.0, 1.0, false)]
        public void CompassData_Comparison(
             double heading1,
             double heading2,
             bool equals)
        {
            var data1 = new CompassData(heading1);
            var data2 = new CompassData(heading2);

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
