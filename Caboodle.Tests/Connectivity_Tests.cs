using Microsoft.Caboodle;
using Xunit;

namespace Caboodle.Tests
{
    public class Connectivity_Tests
    {
        [Fact]
        public void Network_Access_On_NetStandard() =>
            Assert.Throws<NotImplementedInReferenceAssemblyException>(() => Connectivity.NetworkAccess);

        [Fact]
        public void Profiles_On_NetStandard() =>
            Assert.Throws<NotImplementedInReferenceAssemblyException>(() => Connectivity.Profiles);

        [Fact]
        public void Connectivity_Changed_Event_On_NetStandard() =>
            Assert.Throws<NotImplementedInReferenceAssemblyException>(() => Connectivity.ConnectivityChanged += Connectivity_ConnectivityChanged);

        void Connectivity_ConnectivityChanged(ConnectivityChangedEventArgs e)
        {
        }
    }
}
