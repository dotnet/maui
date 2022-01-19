#if !(MACCATALYST || MACOS)
using CoreTelephony;
#endif
using System;
using System.Collections.Generic;

namespace Microsoft.Maui.Essentials
{
	public static partial class Connectivity
	{
#if !(MACCATALYST || MACOS)
		// TODO: Use NWPathMonitor on > iOS 12
#pragma warning disable BI1234
		static readonly Lazy<CTCellularData> cellularData = new Lazy<CTCellularData>(() => new CTCellularData());

		internal static CTCellularData CellularData => cellularData.Value;
#pragma warning restore BI1234
#endif

		static ReachabilityListener listener;

		static void StartListeners()
		{
			listener = new ReachabilityListener();
			listener.ReachabilityChanged += OnConnectivityChanged;
		}

		static void StopListeners()
		{
			if (listener == null)
				return;

			listener.ReachabilityChanged -= OnConnectivityChanged;
			listener.Dispose();
			listener = null;
		}

		static NetworkAccess PlatformNetworkAccess
		{
			get
			{
				var restricted = false;
#if !(MACCATALYST || MACOS)
				// TODO: Use NWPathMonitor on > iOS 12
#pragma warning disable BI1234
				restricted = CellularData.RestrictedState == CTCellularDataRestrictedState.Restricted;
#pragma warning restore BI1234
#endif
				var internetStatus = Reachability.InternetConnectionStatus();
				if ((internetStatus == NetworkStatus.ReachableViaCarrierDataNetwork && !restricted) || internetStatus == NetworkStatus.ReachableViaWiFiNetwork)
					return NetworkAccess.Internet;

				var remoteHostStatus = Reachability.RemoteHostStatus();
				if ((remoteHostStatus == NetworkStatus.ReachableViaCarrierDataNetwork && !restricted) || remoteHostStatus == NetworkStatus.ReachableViaWiFiNetwork)
					return NetworkAccess.Internet;

				return NetworkAccess.None;
			}
		}

		static IEnumerable<ConnectionProfile> PlatformConnectionProfiles
		{
			get
			{
				var statuses = Reachability.GetActiveConnectionType();
				foreach (var status in statuses)
				{
					switch (status)
					{
						case NetworkStatus.ReachableViaCarrierDataNetwork:
							yield return ConnectionProfile.Cellular;
							break;
						case NetworkStatus.ReachableViaWiFiNetwork:
							yield return ConnectionProfile.WiFi;
							break;
						default:
							yield return ConnectionProfile.Unknown;
							break;
					}
				}
			}
		}
	}
}
