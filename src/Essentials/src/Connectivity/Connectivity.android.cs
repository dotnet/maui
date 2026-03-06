using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Net;
using Microsoft.Maui.ApplicationModel;
using Debug = System.Diagnostics.Debug;

namespace Microsoft.Maui.Networking
{
	partial class ConnectivityImplementation : IConnectivity
	{
		/// <summary>
		/// Unique identifier for the connectivity changed action on Android.
		/// </summary>
		public const string ConnectivityChangedAction = "com.maui.essentials.ESSENTIALS_CONNECTIVITY_CHANGED";
		static ConnectivityManager ConnectivityManager =>
			Application.Context.GetSystemService(Context.ConnectivityService) as ConnectivityManager;

		ConnectivityBroadcastReceiver conectivityReceiver;
		EssentialsNetworkCallback networkCallback;

		void StartListeners()
		{
			Permissions.EnsureDeclared<Permissions.NetworkState>();

			var filter = new IntentFilter();

			if (OperatingSystem.IsAndroidVersionAtLeast(24))
			{
				RegisterNetworkCallback();
				filter.AddAction(ConnectivityChangedAction);
			}
			else
			{
#pragma warning disable CS0618 // Type or member is obsolete
				filter.AddAction(ConnectivityManager.ConnectivityAction);
#pragma warning restore CS0618 // Type or member is obsolete
			}

			conectivityReceiver = new ConnectivityBroadcastReceiver(OnConnectivityChanged);

			PlatformUtils.RegisterBroadcastReceiver(conectivityReceiver, filter);
		}

		void StopListeners()
		{
			if (conectivityReceiver == null)
			{
				return;
			}

			try
			{
				UnregisterNetworkCallback();
			}
			catch
			{
				Debug.WriteLine("Connectivity receiver already unregistered. Disposing of it.");
			}

			try
			{
				Application.Context.UnregisterReceiver(conectivityReceiver);
			}
			catch (Java.Lang.IllegalArgumentException)
			{
				Debug.WriteLine("Connectivity receiver already unregistered. Disposing of it.");
			}

			conectivityReceiver = null;
		}

		void RegisterNetworkCallback()
		{
			if (!OperatingSystem.IsAndroidVersionAtLeast(24))
			{
				return;
			}

			var manager = ConnectivityManager;
			if (manager == null)
			{
				return;
			}

			var request = new NetworkRequest.Builder().Build();
			networkCallback = new EssentialsNetworkCallback();
			manager.RegisterNetworkCallback(request, networkCallback);
		}

		void UnregisterNetworkCallback()
		{
			if (!OperatingSystem.IsAndroidVersionAtLeast(24))
			{
				return;
			}

			var manager = ConnectivityManager;
			if (manager == null || networkCallback == null)
			{
				return;
			}

			manager.UnregisterNetworkCallback(networkCallback);

			networkCallback = null;
		}

		class EssentialsNetworkCallback : ConnectivityManager.NetworkCallback
		{
			readonly Intent connectivityIntent;

			public EssentialsNetworkCallback()
			{
				connectivityIntent = new Intent(ConnectivityChangedAction);
				connectivityIntent.SetPackage(Application.Context.PackageName);
			}

			public override void OnAvailable(Network network) =>
				Application.Context.SendBroadcast(connectivityIntent);

			public override void OnLost(Network network) =>
				Application.Context.SendBroadcast(connectivityIntent);

			public override void OnCapabilitiesChanged(Network network, NetworkCapabilities networkCapabilities) =>
				Application.Context.SendBroadcast(connectivityIntent);

			public override void OnUnavailable() =>
				Application.Context.SendBroadcast(connectivityIntent);

			public override void OnLinkPropertiesChanged(Network network, LinkProperties linkProperties) =>
				Application.Context.SendBroadcast(connectivityIntent);

			public override void OnLosing(Network network, int maxMsToLive) =>
				Application.Context.SendBroadcast(connectivityIntent);
		}

		static NetworkAccess IsBetterAccess(NetworkAccess currentAccess, NetworkAccess newAccess) =>
			newAccess > currentAccess ? newAccess : currentAccess;

		public NetworkAccess NetworkAccess
		{
			get
			{
				Permissions.EnsureDeclared<Permissions.NetworkState>();

				try
				{
					var currentAccess = NetworkAccess.None;
					var manager = ConnectivityManager;

#pragma warning disable CA1422 // Validate platform compatibility
					var networks = manager.GetAllNetworks();
#pragma warning restore CA1422 // Validate platform compatibility

					// some devices running 21 and 22 only use the older api.
					if (networks.Length == 0 && !OperatingSystem.IsAndroidVersionAtLeast(23))
					{
						ProcessAllNetworkInfo();
						return currentAccess;
					}

					foreach (var network in networks)
					{
						try
						{
							var capabilities = manager.GetNetworkCapabilities(network);

							if (capabilities == null)
							{
								continue;
							}

							// Check to see if it has the internet capability
							if (!capabilities.HasCapability(NetCapability.Internet))
							{
								// Doesn't have internet, but local is possible
								currentAccess = IsBetterAccess(currentAccess, NetworkAccess.Local);
								continue;
							}

							// Use modern NetworkCapabilities instead of obsolete NetworkInfo
							ProcessNetworkCapabilities(capabilities);
						}
						catch (Exception ex)
						{
							Debug.WriteLine("Failed to get network capabilities: {0}", ex);
						}
					}

					void ProcessAllNetworkInfo()
					{
						// Fallback for API 21-22 devices where GetAllNetworks() returns empty.
						// BEHAVIORAL CHANGE: The original code used GetAllNetworkInfo() which
						// enumerated all network interfaces and picked the best access level.
						// ActiveNetworkInfo returns only the current default network, so a
						// better-connected secondary interface will no longer be considered.
						// On these older API levels multi-network support is limited,
						// so this is an acceptable trade-off.
						try
						{
#pragma warning disable CS0618 // Type or member is obsolete
							var activeInfo = manager.ActiveNetworkInfo;
							if (activeInfo != null)
							{
								ProcessNetworkInfo(activeInfo);
							}
#pragma warning restore CS0618 // Type or member is obsolete
						}
						catch (Exception ex)
						{
							Debug.WriteLine("Failed to query active network info: {0}", ex);
							currentAccess = NetworkAccess.None;
						}
					}

#pragma warning disable CS0618 // Type or member is obsolete
					void ProcessNetworkInfo(NetworkInfo info)
					{
						if (info == null || !info.IsAvailable)
						{
							return;
						}

						if (info.IsConnected)
						{
							currentAccess = IsBetterAccess(currentAccess, NetworkAccess.Internet);
						}
						else if (info.IsConnectedOrConnecting)
						{
							currentAccess = IsBetterAccess(currentAccess, NetworkAccess.ConstrainedInternet);
						}
					}
#pragma warning restore CS0618 // Type or member is obsolete

					// Caller guarantees capabilities is non-null and has NetCapability.Internet.
					void ProcessNetworkCapabilities(NetworkCapabilities capabilities)
					{
						// NetCapability.Validated (API 23+) means the system has verified the
						// network can actually reach the internet (passes Google's connectivity check).
						// Without it, the network may be behind a captive portal or otherwise unusable.
						if (OperatingSystem.IsAndroidVersionAtLeast(23))
						{
							if (capabilities.HasCapability(NetCapability.Validated))
							{
								currentAccess = IsBetterAccess(currentAccess, NetworkAccess.Internet);
							}
							else
							{
								// Has internet capability but not validated (e.g. captive portal)
								currentAccess = IsBetterAccess(currentAccess, NetworkAccess.ConstrainedInternet);
							}
						}
						else
						{
							// NetCapability.Validated is unavailable on API 21-22;
							// treat Internet capability alone as a connected state.
							currentAccess = IsBetterAccess(currentAccess, NetworkAccess.Internet);
						}
					}

					return currentAccess;
				}
				catch (Exception e)
				{
					// TODO add Logging here
					Debug.WriteLine("Unable to get connected state - do you have ACCESS_NETWORK_STATE permission? - error: {0}", e);
					return NetworkAccess.Unknown;
				}
			}
		}

		public IEnumerable<ConnectionProfile> ConnectionProfiles
		{
			get
			{
				Permissions.EnsureDeclared<Permissions.NetworkState>();

				var manager = ConnectivityManager;
#pragma warning disable CA1422 // Validate platform compatibility
				var networks = manager.GetAllNetworks();
#pragma warning restore CA1422 // Validate platform compatibility
				foreach (var network in networks)
				{
					NetworkCapabilities capabilities = null;
					try
					{
						capabilities = manager.GetNetworkCapabilities(network);
					}
					catch (Exception ex)
					{
						Debug.WriteLine("Failed to get network capabilities for profile: {0}", ex);
					}

					var p = ProcessNetworkCapabilities(capabilities);
					if (p.HasValue)
					{
						yield return p.Value;
					}
				}

				static ConnectionProfile? ProcessNetworkCapabilities(NetworkCapabilities capabilities)
				{
					if (capabilities == null)
					{
						return null;
					}

					// Only include networks that declare internet capability
					if (!capabilities.HasCapability(NetCapability.Internet))
					{
						return null;
					}

					return GetConnectionTypeFromCapabilities(capabilities);
				}
			}
		}

		internal static ConnectionProfile GetConnectionTypeFromCapabilities(NetworkCapabilities capabilities)
		{
			if (capabilities == null)
			{
				return ConnectionProfile.Unknown;
			}

			// A network may advertise multiple transports (e.g. WiFi + VPN).
			// We return the first "underlying" transport match, prioritising the
			// physical link type over overlay transports like VPN.
			if (capabilities.HasTransport(TransportType.Wifi))
			{
				return ConnectionProfile.WiFi;
			}

			if (capabilities.HasTransport(TransportType.Cellular))
			{
				return ConnectionProfile.Cellular;
			}

			if (capabilities.HasTransport(TransportType.Ethernet))
			{
				return ConnectionProfile.Ethernet;
			}

			if (capabilities.HasTransport(TransportType.Bluetooth))
			{
				return ConnectionProfile.Bluetooth;
			}

			return ConnectionProfile.Unknown;
		}

		internal static ConnectionProfile GetConnectionType(ConnectivityType connectivityType, string typeName)
		{
			switch (connectivityType)
			{
				case ConnectivityType.Ethernet:
					return ConnectionProfile.Ethernet;
				case ConnectivityType.Wifi:
					return ConnectionProfile.WiFi;
				case ConnectivityType.Bluetooth:
					return ConnectionProfile.Bluetooth;
				case ConnectivityType.Wimax:
				case ConnectivityType.Mobile:
				case ConnectivityType.MobileDun:
				case ConnectivityType.MobileHipri:
				case ConnectivityType.MobileMms:
					return ConnectionProfile.Cellular;
				case ConnectivityType.Dummy:
					return ConnectionProfile.Unknown;
				default:
					if (string.IsNullOrWhiteSpace(typeName))
					{
						return ConnectionProfile.Unknown;
					}

					if (typeName.Contains("mobile", StringComparison.OrdinalIgnoreCase))
					{
						return ConnectionProfile.Cellular;
					}

					if (typeName.Contains("wimax", StringComparison.OrdinalIgnoreCase))
					{
						return ConnectionProfile.Cellular;
					}

					if (typeName.Contains("wifi", StringComparison.OrdinalIgnoreCase))
					{
						return ConnectionProfile.WiFi;
					}

					if (typeName.Contains("ethernet", StringComparison.OrdinalIgnoreCase))
					{
						return ConnectionProfile.Ethernet;
					}

					if (typeName.Contains("bluetooth", StringComparison.OrdinalIgnoreCase))
					{
						return ConnectionProfile.Bluetooth;
					}

					return ConnectionProfile.Unknown;
			}
		}
	}

	[BroadcastReceiver(Enabled = true, Exported = false, Label = "Essentials Connectivity Broadcast Receiver")]
	class ConnectivityBroadcastReceiver : BroadcastReceiver
	{
		Action onChanged;

		/// <summary>
		/// Initializes a new instance of the <see cref="ConnectivityBroadcastReceiver"/> class.
		/// </summary>
		public ConnectivityBroadcastReceiver()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ConnectivityBroadcastReceiver"/> class.
		/// </summary>
		/// <param name="onChanged">The action that is triggered whenever the connectivity changes.</param>
		public ConnectivityBroadcastReceiver(Action onChanged) =>
			this.onChanged = onChanged;

		public override async void OnReceive(Context context, Intent intent)
		{
#pragma warning disable CS0618 // Type or member is obsolete
			if (intent.Action != ConnectivityManager.ConnectivityAction && intent.Action != ConnectivityImplementation.ConnectivityChangedAction)
#pragma warning restore CS0618 // Type or member is obsolete
			{
				return;
			}

			// await 1500ms to ensure that the the connection manager updates
			await Task.Delay(1500);
			onChanged?.Invoke();
		}
	}
}
