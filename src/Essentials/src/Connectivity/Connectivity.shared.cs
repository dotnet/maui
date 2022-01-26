using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Maui.Essentials
{
	/// <include file="../../docs/Microsoft.Maui.Essentials/Connectivity.xml" path="Type[@FullName='Microsoft.Maui.Essentials.Connectivity']/Docs" />
	public static partial class Connectivity
	{
		static event EventHandler<ConnectivityChangedEventArgs> ConnectivityChangedInternal;

		// a cache so that events aren't fired unnecessarily
		// this is mainly an issue on Android, but we can stiil do this everywhere
		static NetworkAccess currentAccess;
		static List<ConnectionProfile> currentProfiles;

		/// <include file="../../docs/Microsoft.Maui.Essentials/Connectivity.xml" path="//Member[@MemberName='NetworkAccess']/Docs" />
		public static NetworkAccess NetworkAccess => PlatformNetworkAccess;

		/// <include file="../../docs/Microsoft.Maui.Essentials/Connectivity.xml" path="//Member[@MemberName='ConnectionProfiles']/Docs" />
		public static IEnumerable<ConnectionProfile> ConnectionProfiles => PlatformConnectionProfiles.Distinct();

		public static event EventHandler<ConnectivityChangedEventArgs> ConnectivityChanged
		{
			add
			{
				var wasRunning = ConnectivityChangedInternal != null;

				ConnectivityChangedInternal += value;

				if (!wasRunning && ConnectivityChangedInternal != null)
				{
					SetCurrent();
					StartListeners();
				}
			}

			remove
			{
				var wasRunning = ConnectivityChangedInternal != null;

				ConnectivityChangedInternal -= value;

				if (wasRunning && ConnectivityChangedInternal == null)
					StopListeners();
			}
		}

		static void SetCurrent()
		{
			currentAccess = NetworkAccess;
			currentProfiles = new List<ConnectionProfile>(ConnectionProfiles);
		}

		static void OnConnectivityChanged(NetworkAccess access, IEnumerable<ConnectionProfile> profiles)
			=> OnConnectivityChanged(new ConnectivityChangedEventArgs(access, profiles));

		static void OnConnectivityChanged()
			=> OnConnectivityChanged(NetworkAccess, ConnectionProfiles);

		static void OnConnectivityChanged(ConnectivityChangedEventArgs e)
		{
			if (currentAccess != e.NetworkAccess || !currentProfiles.SequenceEqual(e.ConnectionProfiles))
			{
				SetCurrent();
				MainThread.BeginInvokeOnMainThread(() => ConnectivityChangedInternal?.Invoke(null, e));
			}
		}
	}

	/// <include file="../../docs/Microsoft.Maui.Essentials/ConnectivityChangedEventArgs.xml" path="Type[@FullName='Microsoft.Maui.Essentials.ConnectivityChangedEventArgs']/Docs" />
	public class ConnectivityChangedEventArgs : EventArgs
	{
		/// <include file="../../docs/Microsoft.Maui.Essentials/ConnectivityChangedEventArgs.xml" path="//Member[@MemberName='.ctor']/Docs" />
		public ConnectivityChangedEventArgs(NetworkAccess access, IEnumerable<ConnectionProfile> connectionProfiles)
		{
			NetworkAccess = access;
			ConnectionProfiles = connectionProfiles;
		}

		/// <include file="../../docs/Microsoft.Maui.Essentials/ConnectivityChangedEventArgs.xml" path="//Member[@MemberName='NetworkAccess']/Docs" />
		public NetworkAccess NetworkAccess { get; }

		/// <include file="../../docs/Microsoft.Maui.Essentials/ConnectivityChangedEventArgs.xml" path="//Member[@MemberName='ConnectionProfiles']/Docs" />
		public IEnumerable<ConnectionProfile> ConnectionProfiles { get; }

		/// <include file="../../docs/Microsoft.Maui.Essentials/ConnectivityChangedEventArgs.xml" path="//Member[@MemberName='ToString']/Docs" />
		public override string ToString() =>
			$"{nameof(NetworkAccess)}: {NetworkAccess}, " +
			$"{nameof(ConnectionProfiles)}: [{string.Join(", ", ConnectionProfiles)}]";
	}
}
