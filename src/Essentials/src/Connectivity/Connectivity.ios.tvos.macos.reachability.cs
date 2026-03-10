using System;
using System.Collections.Generic;
using System.Threading;
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
		static readonly ManualResetEventSlim initEvent = new ManualResetEventSlim(false);

		static NWPathMonitor SharedMonitor
		{
			get
			{
				lock (monitorLock)
				{
					if (sharedMonitor == null)
					{
						sharedMonitor = new NWPathMonitor();
						sharedMonitor.SnapshotHandler = _ => initEvent.Set();
						sharedMonitor.SetQueue(DispatchQueue.DefaultGlobalQueue);
						sharedMonitor.Start();
					}
				}
				// Wait for the first path update to ensure CurrentPath is available.
				// ManualResetEventSlim stays signaled once Set(), so subsequent calls return immediately.
				initEvent.Wait(TimeSpan.FromSeconds(5));
				return sharedMonitor;
			}
		}

		static NWPath GetCurrentPath()
		{
			var monitor = SharedMonitor;
			return monitor?.CurrentPath;
		}

		static NetworkStatus GetNetworkStatus()
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

		// RemoteHostStatus and InternetConnectionStatus previously had different
		// implementations (DNS probe to www.microsoft.com vs default route check).
		// With NWPathMonitor they share the same underlying path status.
		internal static NetworkStatus RemoteHostStatus() => GetNetworkStatus();

		internal static NetworkStatus InternetConnectionStatus() => GetNetworkStatus();

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

		internal ReachabilityListener()
		{
			pathMonitor = new NWPathMonitor();
			pathMonitor.SnapshotHandler = OnPathUpdate;
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
				pathMonitor.SnapshotHandler = null;
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

		async void OnPathUpdate(NWPath path)
		{
			try
			{
				// Add in artificial delay so the connection status has time to change
				await Task.Delay(ConnectionStatusChangeDelayMs);
				ReachabilityChanged?.Invoke();
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"ReachabilityListener handler failed: {ex}");
			}
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
