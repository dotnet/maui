using System;
using Xamarin.Essentials;
using Xunit;

namespace Tests
{
    public class Compass_Tests
    {
        public Compass_Tests()
        {
            Compass.Stop();
        }

        [Fact]
        public void IsSupported_On_NetStandard() =>
            Assert.Throws<NotImplementedInReferenceAssemblyException>(() => Compass.IsSupported);

        [Fact]
        public void Monitor_Null_Handler_On_NetStandard() =>
            Assert.Throws<ArgumentNullException>(() => Compass.Start(SensorSpeed.Normal));

        [Fact]
        public void Monitor_On_NetStandard() =>
            Assert.Throws<NotImplementedInReferenceAssemblyException>(() => Compass.Start(SensorSpeed.Normal));

        [Fact]
        public void IsMonitoring_Default_On_NetStandard() =>
            Assert.False(Compass.IsMonitoring);

        [Fact]
        public void CompassData_Equals_AreSameCopy()
        {
            var data = new CompassData(0);
            var copy = data;
            var res = data.Equals(copy);
            Assert.True(res);
        }

        [Fact]
        public void CompassData_Equals_AreSameValues()
        {
            var data = new CompassData(0);
            var copy = new CompassData(0);
            Assert.True(data.Equals(copy));
        }

        [Fact]
        public void CompassData_Equals_AreDifferent()
        {
            var data = new CompassData(0);
            var copy = new CompassData(1);
            Assert.False(data.Equals(copy));
        }

        [Fact]
        public void CompassData_Equals_Operator_AreSameValues()
        {
            var data = new CompassData(0);
            var copy = new CompassData(0);
            Assert.True(data == copy);
            Assert.False(data != copy);
        }

        [Fact]
        public void CompassData_Equals_Operator_AreDifferent()
        {
            var data = new CompassData(0);
            var copy = new CompassData(1);
            Assert.False(data == copy);
            Assert.True(data != copy);
        }
    }
}
