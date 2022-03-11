using System.Collections.Generic;
using Tizen.Network.Connection;

namespace Microsoft.Maui.Essentials.Implementations
{
	public partial class ConnectivityImplementation : IConnectivity
	{
		static IList<ConnectionProfile> profiles = new List<ConnectionProfile>();

		internal static void OnChanged(object sender, object e)
		{
			GetProfileListAsync();
		}

		internal static async void GetProfileListAsync()
		{
			var list = await ConnectionProfileManager.GetProfileListAsync(ProfileListType.Connected);
			profiles.Clear();
			foreach (var result in list)
			{
				switch (result.Type)
				{
					case ConnectionProfileType.Bt:
						profiles.Add(ConnectionProfile.Bluetooth);
						break;

					case ConnectionProfileType.Cellular:
						profiles.Add(ConnectionProfile.Cellular);
						break;

					case ConnectionProfileType.Ethernet:
						profiles.Add(ConnectionProfile.Ethernet);
						break;

					case ConnectionProfileType.WiFi:
						profiles.Add(ConnectionProfile.WiFi);
						break;
				}
			}
			Connectivity.OnConnectivityChanged();
		}

		public NetworkAccess NetworkAccess
		{
			get
			{
				Permissions.EnsureDeclared<Permissions.NetworkState>();
				var currentAccess = ConnectionManager.CurrentConnection;
				switch (currentAccess.Type)
				{
					case ConnectionType.WiFi:
					case ConnectionType.Cellular:
					case ConnectionType.Ethernet:
						return NetworkAccess.Internet;
					default:
						return NetworkAccess.None;
				}
			}
		}

		public IEnumerable<ConnectionProfile> ConnectionProfiles
		{
			get
			{
				return profiles;
			}
		}

		public void StartListeners()
		{
			Permissions.EnsureDeclared<Permissions.NetworkState>();
			ConnectionManager.ConnectionTypeChanged += OnChanged;
			GetProfileListAsync();
		}

		public void StopListeners()
		{
			ConnectionManager.ConnectionTypeChanged -= OnChanged;
		}
	}
}
