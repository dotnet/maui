using Microsoft.Caboodle;
using Xunit;

namespace Caboodle.Tests
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
    }
}
