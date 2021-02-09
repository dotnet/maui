using Xamarin.Essentials;
using Xunit;

namespace Tests
{
	public class Connectivity_Tests
	{
		[Fact]
		public void Network_Access_On_NetStandard() =>
			Assert.Throws<NotImplementedInReferenceAssemblyException>(() => Connectivity.NetworkAccess);

		[Fact]
		public void ConnectionProfiles_On_NetStandard() =>
			Assert.Throws<NotImplementedInReferenceAssemblyException>(() => Connectivity.ConnectionProfiles);

		[Fact]
		public void Connectivity_Changed_Event_On_NetStandard() =>
			Assert.Throws<NotImplementedInReferenceAssemblyException>(() => Connectivity.ConnectivityChanged += Connectivity_ConnectivityChanged);

		void Connectivity_ConnectivityChanged(object sender, ConnectivityChangedEventArgs e)
		{
		}
	}
}
