using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Networking;

namespace Microsoft.Maui.Networking
{
	/// <summary>
	/// The Connectivity API lets you monitor for changes in the device's network conditions, check the current network access, and determine how it is currently connected.
	/// </summary>
	public interface IConnectivity
	{
		/// <summary>
		/// Gets the active connectivity types for the device.
		/// </summary>
		IEnumerable<ConnectionProfile> ConnectionProfiles { get; }

		/// <summary>
		/// Gets the current state of network access.
		/// </summary>
		NetworkAccess NetworkAccess { get; }

		/// <summary>
		/// Occurs when network access or profile has changed.
		/// </summary>
		event EventHandler<ConnectivityChangedEventArgs> ConnectivityChanged;
	}

#nullable enable
	/// <summary>
	/// The Connectivity APIs lets you monitor for changes in the device's network conditions, check the current network access, and how it is currently connected.
	/// </summary>
	public static class Connectivity
	{
		/// <summary>
		/// Gets the current state of network access.
		/// </summary>
		/// <remarks>
		/// <para>Even when <see cref="NetworkAccess.Internet"/> is returned, full internet access is not guaranteed.</para>
		/// <para>Can throw <see cref="PermissionException"/> on Android if <c>ACCESS_NETWORK_STATE</c> is not set in manifest.</para>
		/// </remarks>
		public static NetworkAccess NetworkAccess => Current.NetworkAccess;

		/// <summary>
		/// Gets the active connectivity types for the device.
		/// </summary>
		/// <remarks>Can throw <see cref="PermissionException"/> on Android if <c>ACCESS_NETWORK_STATE</c> is not set in manifest.</remarks>
		public static IEnumerable<ConnectionProfile> ConnectionProfiles => Current.ConnectionProfiles.Distinct();

		/// <summary>
		/// Occurs when network access or profile has changed.
		/// </summary>
		/// <remarks>Can throw <see cref="PermissionException"/> on Android if <c>ACCESS_NETWORK_STATE</c> is not set in manifest.</remarks>
		public static event EventHandler<ConnectivityChangedEventArgs> ConnectivityChanged
		{
			add => Current.ConnectivityChanged += value;
			remove => Current.ConnectivityChanged -= value;
		}

		static IConnectivity? currentImplementation;

		/// <summary>
		/// Provides the default implementation for static usage of this API.
		/// </summary>
		public static IConnectivity Current =>
			currentImplementation ??= new ConnectivityImplementation();

		internal static void SetCurrent(IConnectivity? implementation) =>
			currentImplementation = implementation;
	}
#nullable disable

	partial class ConnectivityImplementation : IConnectivity
	{
		event EventHandler<ConnectivityChangedEventArgs> ConnectivityChangedInternal;

		// a cache so that events aren't fired unnecessarily
		// this is mainly an issue on Android, but we can still do this everywhere
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

	/// <summary>
	/// The current connectivity information from the change event.
	/// </summary>
	public class ConnectivityChangedEventArgs : EventArgs
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="ConnectivityChangedEventArgs"/> class.
		/// </summary>
		/// <param name="access">The current access of the network.</param>
		/// <param name="connectionProfiles">The connection profiles changing correspondingto this event.</param>
		public ConnectivityChangedEventArgs(NetworkAccess access, IEnumerable<ConnectionProfile> connectionProfiles)
		{
			NetworkAccess = access;
			ConnectionProfiles = connectionProfiles;
		}

		/// <summary>
		/// Gets the current state of network access.
		/// </summary>
		/// <remarks>
		/// <para>Even when <see cref="NetworkAccess.Internet"/> is returned, full internet access is not guaranteed.</para>
		/// <para>Can throw <see cref="PermissionException"/> on Android if <c>ACCESS_NETWORK_STATE</c> is not set in manifest.</para>
		/// </remarks>
		public NetworkAccess NetworkAccess { get; }

		/// <summary>
		/// Gets the active connectivity profiles for the device.
		/// </summary>
		public IEnumerable<ConnectionProfile> ConnectionProfiles { get; }

		/// <summary>
		/// Returns a string representation of the current values of <see cref="ConnectivityChangedEventArgs"/>.
		/// </summary>
		/// <returns>A string representation of this instance in the format of <c>NetworkAccess: {value}, ConnectionProfiles: [{value1}, {value2}]</c>.</returns>
		public override string ToString() =>
			$"{nameof(NetworkAccess)}: {NetworkAccess}, " +
			$"{nameof(ConnectionProfiles)}: [{string.Join(", ", ConnectionProfiles)}]";
	}
}
