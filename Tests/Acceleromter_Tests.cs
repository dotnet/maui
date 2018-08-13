using Xamarin.Essentials;
using Xunit;

namespace Tests
{
    public class Accelerometer_Tests
    {
        public Accelerometer_Tests()
        {
            Accelerometer.Stop();
        }

        [Fact]
        public void IsSupported_On_NetStandard() =>
            Assert.Throws<NotImplementedInReferenceAssemblyException>(() => Accelerometer.IsSupported);

        [Fact]
        public void Monitor_On_NetStandard() =>
            Assert.Throws<NotImplementedInReferenceAssemblyException>(() => Accelerometer.Start(SensorSpeed.Normal));

        [Fact]
        public void IsMonitoring_Default_On_NetStandard() =>
            Assert.False(Accelerometer.IsMonitoring);

        [Fact]
        public void AccelerometerData_Equals_AreSameCopy()
        {
            var data = new AccelerometerData(0, 0, 0);
            var copy = data;
            var res = data.Equals(copy);
            Assert.True(res);
        }

        [Fact]
        public void AccelerometerData_Equals_AreSameValues()
        {
            var data = new AccelerometerData(0, 0, 0);
            var copy = new AccelerometerData(0, 0, 0);
            Assert.True(data.Equals(copy));
        }

        [Fact]
        public void AccelerometerData_Equals_AreDifferent()
        {
            var data = new AccelerometerData(0, 0, 0);
            var copy = new AccelerometerData(0, 0, 1);
            Assert.False(data.Equals(copy));
        }

        [Fact]
        public void AccelerometerData_Equals_Operator_AreSameValues()
        {
            var data = new AccelerometerData(0, 0, 0);
            var copy = new AccelerometerData(0, 0, 0);
            Assert.True(data == copy);
            Assert.False(data != copy);
        }

        [Fact]
        public void AccelerometerData_Equals_Operator_AreDifferent()
        {
            var data = new AccelerometerData(0, 0, 0);
            var copy = new AccelerometerData(0, 0, 1);
            Assert.False(data == copy);
            Assert.True(data != copy);
        }
    }
}
