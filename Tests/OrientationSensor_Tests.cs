using Xamarin.Essentials;
using Xunit;

namespace Tests
{
    public class OrientationSensor_Tests
    {
        [Fact]
        public void OrientationSensorData_Equals_AreSameCopy()
        {
            var data = new OrientationSensorData(0, 0, 0, 0);
            var copy = data;
            var res = data.Equals(copy);
            Assert.True(res);
        }

        [Fact]
        public void OrientationSensorData_Equals_AreSameValues()
        {
            var data = new OrientationSensorData(0, 0, 0, 0);
            var copy = new OrientationSensorData(0, 0, 0, 0);
            Assert.True(data.Equals(copy));
        }

        [Fact]
        public void OrientationSensorData_Equals_AreDifferent()
        {
            var data = new OrientationSensorData(0, 0, 0, 0);
            var copy = new OrientationSensorData(0, 0, 0, 1);
            Assert.False(data.Equals(copy));
        }

        [Fact]
        public void OrientationSensorData_Equals_Operator_AreSameValues()
        {
            var data = new OrientationSensorData(0, 0, 0, 0);
            var copy = new OrientationSensorData(0, 0, 0, 0);
            Assert.True(data == copy);
            Assert.False(data != copy);
        }

        [Fact]
        public void OrientationSensorData_Equals_Operator_AreDifferent()
        {
            var data = new OrientationSensorData(0, 0, 0, 0);
            var copy = new OrientationSensorData(0, 0, 0, 1);
            Assert.False(data == copy);
            Assert.True(data != copy);
        }
    }
}
