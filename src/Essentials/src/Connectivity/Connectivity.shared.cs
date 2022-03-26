using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using Microsoft.Maui.Networking;
using Microsoft.Maui.ApplicationModel;

namespace Microsoft.Maui.Networking
{
	public interface IConnectivity
	{
		IEnumerable<ConnectionProfile> ConnectionProfiles { get; }

		NetworkAccess NetworkAccess { get; }

		event EventHandler<ConnectivityChangedEventArgs> ConnectivityChanged;
	}

#nullable enable
	/// <include file="../../docs/Microsoft.Maui.Essentials/Connectivity.xml" path="Type[@FullName='Microsoft.Maui.Essentials.Connectivity']/Docs" />
	public static class Connectivity
	{
		/// <include file="../../docs/Microsoft.Maui.Essentials/Connectivity.xml" path="//Member[@MemberName='NetworkAccess']/Docs" />
		public static NetworkAccess NetworkAccess => Current.NetworkAccess;

		/// <include file="../../docs/Microsoft.Maui.Essentials/Connectivity.xml" path="//Member[@MemberName='ConnectionProfiles']/Docs" />
		public static IEnumerable<ConnectionProfile> ConnectionProfiles => Current.ConnectionProfiles.Distinct();

		/// <include file="../../docs/Microsoft.Maui.Essentials/Connectivity.xml" path="//Member[@MemberName='ConnectivityChanged']/Docs" />
		public static event EventHandler<ConnectivityChangedEventArgs> ConnectivityChanged
		{
			add => Current.ConnectivityChanged += value;
			remove => Current.ConnectivityChanged -= value;
		}

		static IConnectivity? currentImplementation;

		public static IConnectivity Current =>
			currentImplementation ??= new ConnectivityImplementation();

		internal static void SetCurrent(IConnectivity? implementation) =>
			currentImplementation = implementation;
	}
#nullable disable

	/// <include file="../../docs/Microsoft.Maui.Essentials/Connectivity.xml" path="Type[@FullName='Microsoft.Maui.Essentials.Connectivity']/Docs" />
	partial class ConnectivityImplementation : IConnectivity
	{
		event EventHandler<ConnectivityChangedEventArgs> ConnectivityChangedInternal;

		// a cache so that events aren't fired unnecessarily
		// this is mainly an issue on Android, but we can stiil do this everywhere
		NetworkAccess currentAccess;
		List<ConnectionProfile> currentProfiles;

		public event EventHandler<ConnectivityChangedEventArgs> ConnectivityChanged
		{
			add
			{
				if (ConnectivityChangedInternal is null)
				{
					SetCurrent();
					StartListeners();
				}
				ConnectivityChangedInternal += value;
			}
			remove
			{
				ConnectivityChangedInternal -= value;
				if (ConnectivityChangedInternal is null)
					StopListeners();
			}
		}

		void SetCurrent()
		{
			currentAccess = NetworkAccess;
			currentProfiles = new List<ConnectionProfile>(ConnectionProfiles);
		}

		void OnConnectivityChanged(NetworkAccess access, IEnumerable<ConnectionProfile> profiles)
			=> OnConnectivityChanged(new ConnectivityChangedEventArgs(access, profiles));

		void OnConnectivityChanged()
			=> OnConnectivityChanged(NetworkAccess, ConnectionProfiles);

		void OnConnectivityChanged(ConnectivityChangedEventArgs e)
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
