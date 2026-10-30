using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Maui.Networking;

namespace Microsoft.Maui.Essentials.Gtk;

/// <summary>
/// <see cref="IConnectivity"/> for Linux. Reachability comes from <c>Gio.NetworkMonitor</c> (which follows NetworkManager
/// or systemd-networkd through the portal), the connection types from <c>/sys/class/net</c>.
/// </summary>
/// <remarks><see cref="ConnectivityChanged"/> is raised on the GLib main loop thread.</remarks>
sealed class GtkConnectivity : IConnectivity
{
	readonly string sysClassNet;
	GLib.INetworkMonitor? monitor;
	GLib.NetworkChangedHandler? handler;
	EventHandler<ConnectivityChangedEventArgs>? changed;
	int subscribers;

	internal GtkConnectivity(string? sysClassNet = null) =>
		this.sysClassNet = sysClassNet ?? "/sys/class/net";

	public NetworkAccess NetworkAccess
	{
		get
		{
			try
			{
				var m = GLib.NetworkMonitorAdapter.Default;
				return ToNetworkAccess(m.NetworkAvailable, m.Connectivity);
			}
			catch (Exception)
			{
				return NetworkAccess.Unknown;
			}
		}
	}

	public IEnumerable<ConnectionProfile> ConnectionProfiles => ReadProfiles(sysClassNet);

	public event EventHandler<ConnectivityChangedEventArgs> ConnectivityChanged
	{
		add
		{
			lock (this)
			{
				changed += value;
				if (subscribers++ == 0)
					StartMonitoring();
			}
		}

		remove
		{
			lock (this)
			{
				changed -= value;
				if (subscribers > 0 && --subscribers == 0)
					StopMonitoring();
			}
		}
	}

	internal static NetworkAccess ToNetworkAccess(bool available, GLib.NetworkConnectivity connectivity)
	{
		if (!available)
			return NetworkAccess.None;

		return connectivity switch
		{
			GLib.NetworkConnectivity.Full => NetworkAccess.Internet,
			GLib.NetworkConnectivity.Portal => NetworkAccess.ConstrainedInternet,
			GLib.NetworkConnectivity.Limited => NetworkAccess.Local,
			GLib.NetworkConnectivity.Local => NetworkAccess.Local,
			_ => NetworkAccess.Unknown,
		};
	}

	/// <summary>
	/// Classifies the interfaces that are up: <c>wireless</c>/<c>phy80211</c> is Wi-Fi, <c>ww*</c> cellular, <c>bnep*</c> Bluetooth,
	/// other physical (non-virtual) interfaces Ethernet. Loopback and virtual interfaces (docker, veth, bridges, tun) are ignored.
	/// </summary>
	internal static IEnumerable<ConnectionProfile> ReadProfiles(string sysClassNet)
	{
		var result = new List<ConnectionProfile>();
		if (!Directory.Exists(sysClassNet))
			return result;

		foreach (var dir in Directory.GetDirectories(sysClassNet))
		{
			var name = Path.GetFileName(dir);
			if (name == "lo")
				continue;

			var state = ReadText(Path.Combine(dir, "operstate"));
			if (state is not null && state != "up" && state != "unknown")
				continue;

			ConnectionProfile? profile = null;
			if (Directory.Exists(Path.Combine(dir, "wireless")) || Directory.Exists(Path.Combine(dir, "phy80211")))
				profile = ConnectionProfile.WiFi;
			else if (name.StartsWith("ww", StringComparison.Ordinal))
				profile = ConnectionProfile.Cellular;
			else if (name.StartsWith("bnep", StringComparison.Ordinal))
				profile = ConnectionProfile.Bluetooth;
			else if (ReadText(Path.Combine(dir, "type")) == "1" && !IsVirtual(dir))
				profile = ConnectionProfile.Ethernet;

			if (profile is { } p && !result.Contains(p))
				result.Add(p);
		}

		return result;
	}

	static bool IsVirtual(string interfaceDir)
	{
		try
		{
			// /sys/class/net/<name> is a symlink into /sys/devices/virtual/net for virtual devices.
			var target = new DirectoryInfo(interfaceDir).ResolveLinkTarget(true)?.FullName ?? interfaceDir;
			return target.Replace('\\', '/').Contains("/devices/virtual/", StringComparison.Ordinal);
		}
		catch (IOException)
		{
			return false;
		}
	}

	static string? ReadText(string path)
	{
		try
		{
			return File.Exists(path) ? File.ReadAllText(path).Trim() : null;
		}
		catch (IOException)
		{
			return null;
		}
		catch (UnauthorizedAccessException)
		{
			return null;
		}
	}

	void StartMonitoring()
	{
		try
		{
			monitor = GLib.NetworkMonitorAdapter.Default;
			handler = (_, _) => changed?.Invoke(this, new ConnectivityChangedEventArgs(NetworkAccess, ConnectionProfiles));
			monitor.NetworkChanged += handler;
		}
		catch (Exception)
		{
			monitor = null;
			handler = null;
		}
	}

	void StopMonitoring()
	{
		if (monitor is not null && handler is not null)
			monitor.NetworkChanged -= handler;

		monitor = null;
		handler = null;
	}
}
