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

		[Fact]
		public async Task Connection_BackgroundThread()
		{
			var current = Connectivity.Current.NetworkAccess;

			var thread = await Task.Run(() => Connectivity.Current.NetworkAccess);

			Assert.Equal(current, thread);
		}
	}
}
