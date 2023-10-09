using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Windows.Networking.Connectivity;
using Microsoft.Maui.ApplicationModel;
using System.Threading.Tasks;
using Microsoft.UI.Dispatching;

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

		public NetworkAccess NetworkAccess => 
			HasThreadAccess ?
			GetNetworkAccess() :			
			DispatchAsync(GetNetworkAccess).GetAwaiter().GetResult();

		bool HasThreadAccess
		{
			get
			{
				DispatcherQueue dispatcherQueue = WindowStateManager.Default?.GetActiveWindow(false)?.DispatcherQueue;

				if (dispatcherQueue is null)
					return true;

				return dispatcherQueue.HasThreadAccess;
			}
		}

		Task<T> DispatchAsync<T>(Func<T> func)
		{
			var dispatcherQueue = WindowStateManager.Default.GetActiveWindow(false)?.DispatcherQueue;

			var tcs = new TaskCompletionSource<T>();

			_ = dispatcherQueue.TryEnqueue(() =>
			{
				try
				{
					var result = func();
					tcs.SetResult(result);
				}
				catch (Exception ex)
				{
					tcs.SetException(ex);
				}
			});

			return tcs.Task;
		}

		NetworkAccess GetNetworkAccess()
		{
			try
			{
				var profile = NetworkInformation.GetInternetConnectionProfile();

				if (profile == null)
					return NetworkAccess.Unknown;

				var level = profile.GetNetworkConnectivityLevel();

				switch (level)
				{
					case NetworkConnectivityLevel.LocalAccess:
						return NetworkAccess.Local;
					case NetworkConnectivityLevel.InternetAccess:
						return NetworkAccess.Internet;
					case NetworkConnectivityLevel.ConstrainedInternetAccess:
						return NetworkAccess.ConstrainedInternet;
					default:
						return NetworkAccess.None;
				}
			}
			catch
			{
				return NetworkAccess.Unknown;
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
						// TODO Add Logging?
						Debug.WriteLine($"Unable to get Network Adapter, returning Unknown: {ex.Message}");
					}

					yield return type;
				}
			}
		}
	}
}
