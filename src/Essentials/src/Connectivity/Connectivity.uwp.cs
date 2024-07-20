using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.NetworkInformation;
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
				if (OperatingSystem.IsWindowsVersionAtLeast(10, 0, 22000, 0))
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
				else
				{
					// Windows 10 workaround for https://github.com/microsoft/WindowsAppSDK/issues/2965
					var networkList = ConnectivityNativeHelper.GetNetworkListManager();
					var enumNetworks = networkList.GetNetworks(ConnectivityNativeHelper.NLM_ENUM_NETWORK.NLM_ENUM_NETWORK_CONNECTED);
					var connectivity = ConnectivityNativeHelper.NLM_CONNECTIVITY.NLM_CONNECTIVITY_DISCONNECTED;

					foreach (ConnectivityNativeHelper.INetwork networkInterface in enumNetworks)
					{
						if (networkInterface.IsConnected())
						{
							connectivity = networkInterface.GetConnectivity();
							break;
						}
					}

					if ((connectivity & (ConnectivityNativeHelper.NLM_CONNECTIVITY.NLM_CONNECTIVITY_IPV4_INTERNET | ConnectivityNativeHelper.NLM_CONNECTIVITY.NLM_CONNECTIVITY_IPV6_INTERNET)) != 0)
					{
						return NetworkAccess.Internet;
					}
					else if ((connectivity & (ConnectivityNativeHelper.NLM_CONNECTIVITY.NLM_CONNECTIVITY_IPV4_LOCALNETWORK | ConnectivityNativeHelper.NLM_CONNECTIVITY.NLM_CONNECTIVITY_IPV6_LOCALNETWORK)) != 0)
					{
						return NetworkAccess.Local;
					}
					else if ((connectivity & (ConnectivityNativeHelper.NLM_CONNECTIVITY.NLM_CONNECTIVITY_IPV4_NOTRAFFIC | ConnectivityNativeHelper.NLM_CONNECTIVITY.NLM_CONNECTIVITY_IPV6_NOTRAFFIC)) != 0)
					{
						return NetworkAccess.Local;
					}
					else
					{
						return NetworkAccess.None;
					}
				}
			}
		}

		public IEnumerable<ConnectionProfile> ConnectionProfiles
		{
			get
			{
				NetworkInterface[] networkInterfaces = [];
				try
				{
					networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
				}
				catch (NetworkInformationException ex)
				{
					Debug.WriteLine($"Unable to get network interfaces. Error: {ex.Message}");
				}

				foreach (var nic in networkInterfaces)
				{
					if (nic.OperationalStatus is not OperationalStatus.Up ||
						nic.NetworkInterfaceType is NetworkInterfaceType.Loopback ||
						nic.NetworkInterfaceType is NetworkInterfaceType.Tunnel)
					{
						continue;
					}

					var interfaceType = ConnectionProfile.Unknown;
					switch (nic.NetworkInterfaceType)
					{
						case NetworkInterfaceType.Ethernet:
							interfaceType = ConnectionProfile.Ethernet;
							break;
						case NetworkInterfaceType.Wireless80211:
							interfaceType = ConnectionProfile.WiFi;
							break;
						case NetworkInterfaceType.Wwanpp:
						case NetworkInterfaceType.Wwanpp2:
							interfaceType = ConnectionProfile.Cellular;
							break;
					}

					yield return interfaceType;
				}
			}
		}
	}
}
