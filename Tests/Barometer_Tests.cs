using System;
using Xamarin.Essentials;
using Xunit;

namespace Tests
{
    public class Barometer_Tests
    {
        [Fact]
        public void IsSupported_On_NetStandard() =>
            Assert.Throws<NotImplementedInReferenceAssemblyException>(() => Barometer.IsSupported);

        [Fact]
        public void Monitor_On_NetStandard() =>
            Assert.Throws<NotImplementedInReferenceAssemblyException>(() => Barometer.Start());

        [Fact]
        public void Monitor_Off_NetStandard() =>
            Assert.Throws<NotImplementedInReferenceAssemblyException>(() => Barometer.Stop());

        [Fact]
        public void IsMonitoring_Default_On_NetStandard() =>
            Assert.False(Barometer.IsMonitoring);
    }
}
