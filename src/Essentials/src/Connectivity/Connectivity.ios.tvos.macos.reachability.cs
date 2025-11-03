using System;
using System.Collections.Generic;
using System.Threading.Tasks;
#if !(MACCATALYST || MACOS)
using CoreTelephony;
#endif
using CoreFoundation;
using Network;

namespace Microsoft.Maui.Networking
{
	enum NetworkStatus
	{
		NotReachable,
		ReachableViaCarrierDataNetwork,
		ReachableViaWiFiNetwork
	}

	static class Reachability
	{
		static NWPathMonitor sharedMonitor;
		static readonly object monitorLock = new object();

		static NWPathMonitor SharedMonitor
		{
			get
			{
				lock (monitorLock)
				{
					if (sharedMonitor == null)
					{
						sharedMonitor = new NWPathMonitor();
						sharedMonitor.SetQueue(DispatchQueue.DefaultGlobalQueue);
						sharedMonitor.Start();
					}
					return sharedMonitor;
				}
			}
		}

		static NWPath GetCurrentPath()
		{
			return SharedMonitor?.CurrentPath;
		}

		internal static NetworkStatus RemoteHostStatus()
		{
			var path = GetCurrentPath();
			if (path == null || path.Status != NWPathStatus.Satisfied)
				return NetworkStatus.NotReachable;

#if __IOS__
			if (path.UsesInterfaceType(NWInterfaceType.Cellular))
				return NetworkStatus.ReachableViaCarrierDataNetwork;
#endif

			return NetworkStatus.ReachableViaWiFiNetwork;
		}

		internal static NetworkStatus InternetConnectionStatus()
		{
			var path = GetCurrentPath();
			if (path == null || path.Status != NWPathStatus.Satisfied)
				return NetworkStatus.NotReachable;

#if __IOS__
			if (path.UsesInterfaceType(NWInterfaceType.Cellular))
				return NetworkStatus.ReachableViaCarrierDataNetwork;
#endif

			return NetworkStatus.ReachableViaWiFiNetwork;
		}

		internal static IEnumerable<NetworkStatus> GetActiveConnectionType()
		{
			var status = new List<NetworkStatus>();
			var path = GetCurrentPath();

			if (path == null || path.Status != NWPathStatus.Satisfied)
				return status;

#if __IOS__
			if (path.UsesInterfaceType(NWInterfaceType.Cellular))
			{
				status.Add(NetworkStatus.ReachableViaCarrierDataNetwork);
			}
			else if (path.UsesInterfaceType(NWInterfaceType.Wifi) || path.UsesInterfaceType(NWInterfaceType.Wired))
#else
			if (path.UsesInterfaceType(NWInterfaceType.Wifi) || path.UsesInterfaceType(NWInterfaceType.Wired))
#endif
			{
				status.Add(NetworkStatus.ReachableViaWiFiNetwork);
			}

			return status;
		}

		internal static bool IsNetworkAvailable()
		{
			var path = GetCurrentPath();
			return path != null && path.Status == NWPathStatus.Satisfied;
		}
	}

	class ReachabilityListener : IDisposable
	{
		// Delay to allow connection status to stabilize before notifying listeners
		const int ConnectionStatusChangeDelayMs = 100;
		
		NWPathMonitor pathMonitor;
		Action<NWPath> pathUpdateHandler;

		internal ReachabilityListener()
		{
			pathMonitor = new NWPathMonitor();
			pathUpdateHandler = async (NWPath path) =>
			{
				// Add in artificial delay so the connection status has time to change
				await Task.Delay(ConnectionStatusChangeDelayMs);
				ReachabilityChanged?.Invoke();
			};
			
			pathMonitor.SnapshotHandler += pathUpdateHandler;
			pathMonitor.SetQueue(DispatchQueue.DefaultGlobalQueue);
			pathMonitor.Start();

#if !(MACCATALYST || MACOS)
#pragma warning disable BI1234, CA1416 // Analyzer bug https://github.com/dotnet/roslyn-analyzers/issues/5938
			ConnectivityImplementation.CellularData.RestrictionDidUpdateNotifier = new Action<CTCellularDataRestrictedState>(OnRestrictedStateChanged);
#pragma warning restore BI1234, CA1416
#endif
		}

		internal event Action ReachabilityChanged;

		void IDisposable.Dispose() => Dispose();

		internal void Dispose()
		{
			if (pathMonitor != null)
			{
				if (pathUpdateHandler != null)
				{
					pathMonitor.SnapshotHandler -= pathUpdateHandler;
					pathUpdateHandler = null;
				}
				pathMonitor.Cancel();
				pathMonitor.Dispose();
				pathMonitor = null;
			}

#if !(MACCATALYST || MACOS)
#pragma warning disable CA1416 // Analyzer bug https://github.com/dotnet/roslyn-analyzers/issues/5938
			ConnectivityImplementation.CellularData.RestrictionDidUpdateNotifier = null;
#pragma warning restore CA1416
#endif
		}

#if !(MACCATALYST || MACOS)
#pragma warning disable BI1234
		void OnRestrictedStateChanged(CTCellularDataRestrictedState state)
		{
			ReachabilityChanged?.Invoke();
		}
#pragma warning restore BI1234
#endif
	}
}
