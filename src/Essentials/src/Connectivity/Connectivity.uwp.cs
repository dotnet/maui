using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Windows.Networking.Connectivity;

namespace Microsoft.Maui.Networking
{
	partial class ConnectivityImplementation : IConnectivity
	{
		void StartListeners() =>
			NetworkInformation.NetworkStatusChanged += NetworkStatusChanged;

		void StopListeners() =>
			NetworkInformation.NetworkStatusChanged -= NetworkStatusChanged;

		void NetworkStatusChanged(object sender) =>
			OnConnectivityChanged();

		public NetworkAccess NetworkAccess
		{
			get
			{
				var profile = NetworkInformation.GetInternetConnectionProfile();
				if (profile == null)
					return NetworkAccess.Unknown;

				var level = profile.GetNetworkConnectivityLevel();
				return level switch
				{
					NetworkConnectivityLevel.LocalAccess => NetworkAccess.Local,
					NetworkConnectivityLevel.InternetAccess => NetworkAccess.Internet,
					NetworkConnectivityLevel.ConstrainedInternetAccess => NetworkAccess.ConstrainedInternet,
					_ => NetworkAccess.None,
				};
			}
		}

		public IEnumerable<ConnectionProfile> ConnectionProfiles
		{
			get
			{
				var networkInterfaceList = NetworkInformation.GetConnectionProfiles();
				foreach (var interfaceInfo in networkInterfaceList.Where(nii => nii.GetNetworkConnectivityLevel() != NetworkConnectivityLevel.None))
				{
					var type = ConnectionProfile.Unknown;

					try
					{
						var adapter = interfaceInfo.NetworkAdapter;
						if (adapter == null)
							continue;

						// http://www.iana.org/assignments/ianaiftype-mib/ianaiftype-mib
						switch (adapter.IanaInterfaceType)
						{
							case 6:
								type = ConnectionProfile.Ethernet;
								break;
							case 71:
								type = ConnectionProfile.WiFi;
								break;
							case 243:
							case 244:
								type = ConnectionProfile.Cellular;
								break;

							// xbox wireless, can skip
							case 281:
								continue;
						}
					}
					catch (Exception ex)
					{
						Debug.WriteLine($"Unable to get Network Adapter, returning Unknown: {ex.Message}");
					}

					yield return type;
				}
			}
		}
	}
}
