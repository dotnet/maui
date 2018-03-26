using System.Linq;
using Microsoft.Caboodle;
using Xunit;

namespace Caboodle.DeviceTests
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
