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
    }
}
