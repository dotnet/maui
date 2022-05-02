using System.Collections.Generic;
using Microsoft.Maui.ApplicationModel;
using Tizen.Network.Connection;

namespace Microsoft.Maui.Networking
{
	partial class ConnectivityImplementation : IConnectivity
	{
		IList<ConnectionProfile> profiles = new List<ConnectionProfile>();

		void OnChanged(object sender, object e)
		{
			GetProfileListAsync();
		}

		async void GetProfileListAsync()
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
			OnConnectivityChanged();
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

		void StartListeners()
		{
			Permissions.EnsureDeclared<Permissions.NetworkState>();
			ConnectionManager.ConnectionTypeChanged += OnChanged;
			GetProfileListAsync();
		}

		void StopListeners()
		{
			ConnectionManager.ConnectionTypeChanged -= OnChanged;
		}
	}
}
