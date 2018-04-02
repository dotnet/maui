using System;
using Microsoft.Caboodle;
using Xunit;

namespace Caboodle.Tests
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
    }
}
