using Xamarin.Essentials;
using Xunit;

namespace Tests
{
    public class Magnetometer_Tests
    {
        public Magnetometer_Tests()
        {
            Magnetometer.Stop();
        }

        [Fact]
        public void IsSupported_On_NetStandard() =>
            Assert.Throws<NotImplementedInReferenceAssemblyException>(() => Magnetometer.IsSupported);

        [Fact]
        public void Monitor_On_NetStandard() =>
            Assert.Throws<NotImplementedInReferenceAssemblyException>(() => Magnetometer.Start(SensorSpeed.Normal));

        [Fact]
        public void IsMonitoring_Default_On_NetStandard() =>
            Assert.False(Magnetometer.IsMonitoring);

        [Fact]
        public void MagnetometerData_Equals_AreSameCopy()
        {
            var data = new MagnetometerData(0, 0, 0);
            var copy = data;
            var res = data.Equals(copy);
            Assert.True(res);
        }

        [Fact]
        public void MagnetometerData_Equals_AreSameValues()
        {
            var data = new MagnetometerData(0, 0, 0);
            var copy = new MagnetometerData(0, 0, 0);
            Assert.True(data.Equals(copy));
        }

        [Fact]
        public void MagnetometerData_Equals_AreDifferent()
        {
            var data = new MagnetometerData(0, 0, 0);
            var copy = new MagnetometerData(0, 0, 1);
            Assert.False(data.Equals(copy));
        }

        [Fact]
        public void MagnetometerData_Equals_Operator_AreSameValues()
        {
            var data = new MagnetometerData(0, 0, 0);
            var copy = new MagnetometerData(0, 0, 0);
            Assert.True(data == copy);
            Assert.False(data != copy);
        }

        [Fact]
        public void MagnetometerData_Equals_Operator_AreDifferent()
        {
            var data = new MagnetometerData(0, 0, 0);
            var copy = new MagnetometerData(0, 0, 1);
            Assert.False(data == copy);
            Assert.True(data != copy);
        }
    }
}
