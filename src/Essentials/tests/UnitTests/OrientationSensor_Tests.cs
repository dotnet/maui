using Xamarin.Essentials;
using Xunit;

namespace Tests
{
    public class OrientationSensor_Tests
    {
        [Fact]
        public void OrientationSensor_Start() =>
           Assert.Throws<NotImplementedInReferenceAssemblyException>(() => OrientationSensor.Stop());

        [Fact]
        public void OrientationSensor_Stop() =>
            Assert.Throws<NotImplementedInReferenceAssemblyException>(() => OrientationSensor.Start(SensorSpeed.Default));

        [Fact]
        public void OrientationSensor_IsMonitoring() =>
            Assert.False(OrientationSensor.IsMonitoring);

        [Fact]
        public void OrientationSensor_IsSupported() =>
            Assert.Throws<NotImplementedInReferenceAssemblyException>(() => OrientationSensor.IsSupported);

        [Theory]
        [InlineData(0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, true)]
        [InlineData(0.0, 0.0, 0.0, 0.0, 1.0, 0.0, 0.0, 0.0, false)]
        [InlineData(0.0, 0.0, 0.0, 0.0, 0.0, 1.0, 0.0, 0.0, false)]
        [InlineData(0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 1.0, 0.0, false)]
        [InlineData(0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 1.0, false)]
        public void OrientationSensorData_Comparison(
            float x1,
            float y1,
            float z1,
            float w1,
            float x2,
            float y2,
            float z2,
            float w2,
            bool equals)
        {
            var data1 = new OrientationSensorData(x1, y1, z1, w1);
            var data2 = new OrientationSensorData(x2, y2, z2, w2);
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
