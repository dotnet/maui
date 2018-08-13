using Xamarin.Essentials;
using Xunit;

namespace Tests
{
    public class Gyroscope_Tests
    {
        public Gyroscope_Tests()
        {
            Gyroscope.Stop();
        }

        [Fact]
        public void IsSupported_On_NetStandard() =>
            Assert.Throws<NotImplementedInReferenceAssemblyException>(() => Gyroscope.IsSupported);

        [Fact]
        public void Monitor_On_NetStandard() =>
            Assert.Throws<NotImplementedInReferenceAssemblyException>(() => Gyroscope.Start(SensorSpeed.Normal));

        [Fact]
        public void IsMonitoring_Default_On_NetStandard() =>
            Assert.False(Gyroscope.IsMonitoring);

        [Fact]
        public void GyroscopeData_Equals_AreSameCopy()
        {
            var data = new GyroscopeData(0, 0, 0);
            var copy = data;
            var res = data.Equals(copy);
            Assert.True(res);
        }

        [Fact]
        public void GyroscopeData_Equals_AreSameValues()
        {
            var data = new GyroscopeData(0, 0, 0);
            var copy = new GyroscopeData(0, 0, 0);
            Assert.True(data.Equals(copy));
        }

        [Fact]
        public void GyroscopeData_Equals_AreDifferent()
        {
            var data = new GyroscopeData(0, 0, 0);
            var copy = new GyroscopeData(0, 0, 1);
            Assert.False(data.Equals(copy));
        }

        [Fact]
        public void GyroscopeData_Equals_Operator_AreSame()
        {
            var data = new GyroscopeData(0, 0, 0);
            var copy = new GyroscopeData(0, 0, 0);
            Assert.True(data == copy);
            Assert.False(data != copy);
        }

        [Fact]
        public void GyroscopeData_Equals_Operator_AreDifferent()
        {
            var data = new GyroscopeData(0, 0, 0);
            var copy = new GyroscopeData(0, 0, 1);
            Assert.False(data == copy);
            Assert.True(data != copy);
        }
    }
}
