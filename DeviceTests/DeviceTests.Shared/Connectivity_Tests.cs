using System.Linq;
using Xamarin.Essentials;
using Xunit;

namespace DeviceTests
{
    public class Connectivity_Tests
    {
        [Fact]
        public void Network_Access() =>
            Assert.Equal(NetworkAccess.Internet, Connectivity.NetworkAccess);

        [Fact]
        public void Profiles() =>
            Assert.True(Connectivity.Profiles.Count() > 0);
    }
}
