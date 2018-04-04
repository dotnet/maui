using Microsoft.Caboodle;
using Xunit;

namespace Caboodle.Tests
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
    }
}
