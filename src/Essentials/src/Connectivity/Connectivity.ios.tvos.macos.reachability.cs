using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
#if __IOS__
using CoreTelephony;
#endif
using CoreFoundation;
using SystemConfiguration;

namespace Microsoft.Maui.Essentials
{
	enum NetworkStatus
	{
		NotReachable,
		ReachableViaCarrierDataNetwork,
		ReachableViaWiFiNetwork
	}

	static class Reachability
	{
		internal const string HostName = "www.microsoft.com";

		internal static NetworkStatus RemoteHostStatus()
		{
			using (var remoteHostReachability = new NetworkReachability(HostName))
			{
				var reachable = remoteHostReachability.TryGetFlags(out var flags);

				if (!reachable)
					return NetworkStatus.NotReachable;

				if (!IsReachableWithoutRequiringConnection(flags))
					return NetworkStatus.NotReachable;

#if __IOS__
				if ((flags & NetworkReachabilityFlags.IsWWAN) != 0)
					return NetworkStatus.ReachableViaCarrierDataNetwork;
#endif

				return NetworkStatus.ReachableViaWiFiNetwork;
			}
		}

		internal static NetworkStatus InternetConnectionStatus()
		{
			var status = NetworkStatus.NotReachable;

			var defaultNetworkAvailable = IsNetworkAvailable(out var flags);

#if __IOS__
			// If it's a WWAN connection..
			if ((flags & NetworkReachabilityFlags.IsWWAN) != 0)
				status = NetworkStatus.ReachableViaCarrierDataNetwork;
#endif

			// If the connection is reachable and no connection is required, then assume it's WiFi
			if (defaultNetworkAvailable)
			{
				status = NetworkStatus.ReachableViaWiFiNetwork;
			}

			// If the connection is on-demand or on-traffic and no user intervention
			// is required, then assume WiFi.
			if (((flags & NetworkReachabilityFlags.ConnectionOnDemand) != 0 || (flags & NetworkReachabilityFlags.ConnectionOnTraffic) != 0) &&
				 (flags & NetworkReachabilityFlags.InterventionRequired) == 0)
			{
				status = NetworkStatus.ReachableViaWiFiNetwork;
			}

			return status;
		}

		internal static IEnumerable<NetworkStatus> GetActiveConnectionType()
		{
			var status = new List<NetworkStatus>();

			var defaultNetworkAvailable = IsNetworkAvailable(out var flags);

#if __IOS__
			// If it's a WWAN connection.
			if ((flags & NetworkReachabilityFlags.IsWWAN) != 0)
			{
				status.Add(NetworkStatus.ReachableViaCarrierDataNetwork);
			}
			else if (defaultNetworkAvailable)
#else
			// If the connection is reachable and no connection is required, then assume it's WiFi
			if (defaultNetworkAvailable)
#endif
			{
				status.Add(NetworkStatus.ReachableViaWiFiNetwork);
			}
			else if (((flags & NetworkReachabilityFlags.ConnectionOnDemand) != 0 || (flags & NetworkReachabilityFlags.ConnectionOnTraffic) != 0) &&
					 (flags & NetworkReachabilityFlags.InterventionRequired) == 0)
			{
				// If the connection is on-demand or on-traffic and no user intervention
				// is required, then assume WiFi.
				status.Add(NetworkStatus.ReachableViaWiFiNetwork);
			}

			return status;
		}

		internal static bool IsNetworkAvailable(out NetworkReachabilityFlags flags)
		{
			var ip = new IPAddress(0);
			using (var defaultRouteReachability = new NetworkReachability(ip))
			{
				if (!defaultRouteReachability.TryGetFlags(out flags))
					return false;

				return IsReachableWithoutRequiringConnection(flags);
			}
		}

		internal static bool IsReachableWithoutRequiringConnection(NetworkReachabilityFlags flags)
		{
			// Is it reachable with the current network configuration?
			var isReachable = (flags & NetworkReachabilityFlags.Reachable) != 0;

			// Do we need a connection to reach it?
			var noConnectionRequired = (flags & NetworkReachabilityFlags.ConnectionRequired) == 0;

#if __IOS__
			// Since the network stack will automatically try to get the WAN up,
			// probe that
			if ((flags & NetworkReachabilityFlags.IsWWAN) != 0)
				noConnectionRequired = true;
#endif

			return isReachable && noConnectionRequired;
		}
	}

	class ReachabilityListener : IDisposable
	{
		NetworkReachability defaultRouteReachability;
		NetworkReachability remoteHostReachability;

		internal ReachabilityListener()
		{
			var ip = new IPAddress(0);
			defaultRouteReachability = new NetworkReachability(ip);
			defaultRouteReachability.SetNotification(OnChange);
			defaultRouteReachability.Schedule(CFRunLoop.Main, CFRunLoop.ModeDefault);

			remoteHostReachability = new NetworkReachability(Reachability.HostName);

			// Need to probe before we queue, or we wont get any meaningful values
			// this only happens when you create NetworkReachability from a hostname
			remoteHostReachability.TryGetFlags(out var flags);

			remoteHostReachability.SetNotification(OnChange);
			remoteHostReachability.Schedule(CFRunLoop.Main, CFRunLoop.ModeDefault);

#if __IOS__
			Connectivity.CellularData.RestrictionDidUpdateNotifier = new Action<CTCellularDataRestrictedState>(OnRestrictedStateChanged);
#endif
		}

		internal event Action ReachabilityChanged;

		void IDisposable.Dispose() => Dispose();

		internal void Dispose()
		{
			defaultRouteReachability?.Dispose();
			defaultRouteReachability = null;
			remoteHostReachability?.Dispose();
			remoteHostReachability = null;

#if __IOS__
			Connectivity.CellularData.RestrictionDidUpdateNotifier = null;
#endif
		}

#if __IOS__
		void OnRestrictedStateChanged(CTCellularDataRestrictedState state)
		{
			ReachabilityChanged?.Invoke();
		}
#endif

		async void OnChange(NetworkReachabilityFlags flags)
		{
			// Add in artifical delay so the connection status has time to change
			// else it will return true no matter what.
			await Task.Delay(100);

			ReachabilityChanged?.Invoke();
		}
	}
}
