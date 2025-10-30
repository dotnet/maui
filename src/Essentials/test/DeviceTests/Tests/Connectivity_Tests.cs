using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Networking;
using Xunit;

namespace Microsoft.Maui.Essentials.DeviceTests
{
	[Category("Connectivity")]
	public class Connectivity_Tests
	{
		[Fact]
		public void Network_Access() =>
			Assert.Equal(NetworkAccess.Internet, Connectivity.NetworkAccess);

		[Fact]
		public void Connection_Profiles() =>
			Assert.True(Connectivity.ConnectionProfiles.Count() > 0);

		[Fact]
		public void Distict_Connection_Profiles()
		{
			var profiles = Connectivity.ConnectionProfiles;
			Assert.Equal(profiles.Count(), profiles.Distinct().Count());
		}

		[Fact(Skip = "This test is failing on net10 preview 5, needs investigation - https://github.com/dotnet/maui/issues/29678")]
		public async Task ConnectivityChanged_Does_Not_Crash()
		{
			Connectivity.ConnectivityChanged += Connectivity_ConnectivityChanged;

			// just ensure there is no need for the OS to "respond" to a new subscription
			await Task.Delay(1000);

			Connectivity.ConnectivityChanged -= Connectivity_ConnectivityChanged;

			static void Connectivity_ConnectivityChanged(object sender, ConnectivityChangedEventArgs e)
			{
				// do nothing
			}
		}

		[Fact]
		public async Task Test()
		{
			var current = Connectivity.Current.NetworkAccess;

			var thread = await Task.Run(() => Connectivity.Current.NetworkAccess).ConfigureAwait(false);

			Assert.Equal(current, thread);
		}
	}
}
