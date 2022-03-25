using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using Microsoft.Maui.ApplicationModel;

namespace Microsoft.Maui.Networking
{
	public interface IConnectivity
	{
		IEnumerable<ConnectionProfile> ConnectionProfiles { get; }

		NetworkAccess NetworkAccess { get; }

		void StartListeners();

		void StopListeners();
	}

	/// <include file="../../docs/Microsoft.Maui.Essentials/Connectivity.xml" path="Type[@FullName='Microsoft.Maui.Essentials.Connectivity']/Docs" />
	public static partial class Connectivity
	{
		static event EventHandler<ConnectivityChangedEventArgs> ConnectivityChangedInternal;

		// a cache so that events aren't fired unnecessarily
		// this is mainly an issue on Android, but we can stiil do this everywhere
		static NetworkAccess currentAccess;
		static List<ConnectionProfile> currentProfiles;

		/// <include file="../../docs/Microsoft.Maui.Essentials/Connectivity.xml" path="//Member[@MemberName='NetworkAccess']/Docs" />
		public static NetworkAccess NetworkAccess => Current.NetworkAccess;

		/// <include file="../../docs/Microsoft.Maui.Essentials/Connectivity.xml" path="//Member[@MemberName='ConnectionProfiles']/Docs" />
		public static IEnumerable<ConnectionProfile> ConnectionProfiles => Current.ConnectionProfiles.Distinct();

		public static event EventHandler<ConnectivityChangedEventArgs> ConnectivityChanged
		{
			add
			{
				var wasRunning = ConnectivityChangedInternal != null;

				ConnectivityChangedInternal += value;

				if (!wasRunning && ConnectivityChangedInternal != null)
				{
					SetCurrent();
					Current.StartListeners();
				}
			}

			remove
			{
				var wasRunning = ConnectivityChangedInternal != null;

				ConnectivityChangedInternal -= value;

				if (wasRunning && ConnectivityChangedInternal == null)
					Current.StopListeners();
			}
		}

		static void SetCurrent()
		{
			currentAccess = NetworkAccess;
			currentProfiles = new List<ConnectionProfile>(ConnectionProfiles);
		}

		internal static void OnConnectivityChanged(NetworkAccess access, IEnumerable<ConnectionProfile> profiles)
			=> OnConnectivityChanged(new ConnectivityChangedEventArgs(access, profiles));

		internal static void OnConnectivityChanged()
			=> OnConnectivityChanged(NetworkAccess, ConnectionProfiles);

		internal static void OnConnectivityChanged(ConnectivityChangedEventArgs e)
		{
			if (currentAccess != e.NetworkAccess || !currentProfiles.SequenceEqual(e.ConnectionProfiles))
			{
				SetCurrent();
				MainThread.BeginInvokeOnMainThread(() => ConnectivityChangedInternal?.Invoke(null, e));
			}
		}

#nullable enable
		static IConnectivity? currentImplementation;
#nullable disable

		[EditorBrowsable(EditorBrowsableState.Never)]
		public static IConnectivity Current =>
			currentImplementation ??= new ConnectivityImplementation();

		[EditorBrowsable(EditorBrowsableState.Never)]
#nullable enable
		public static void SetCurrent(IConnectivity? implementation) =>
			currentImplementation = implementation;
#nullable disable
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
