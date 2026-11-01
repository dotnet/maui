using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Maui.Devices.Sensors;

namespace Microsoft.Maui.Essentials.Gtk;

/// <summary>A fix as GeoClue2 reports it (<c>org.freedesktop.GeoClue2.Location</c>), with its "unknown" sentinels already removed.</summary>
sealed record GeoClueFix(
	double Latitude,
	double Longitude,
	double? Accuracy,
	double? Altitude,
	double? Speed,
	double? Heading,
	DateTimeOffset Timestamp)
{
	/// <summary>GeoClue reports an unknown altitude as <c>-DBL_MAX</c>; anything below this is "unknown".</summary>
	const double UnknownAltitudeBelow = -1e300;

	/// <summary>Converts to the MAUI type; altitude is metres above sea level, so the reference system is the geoid.</summary>
	internal Location ToLocation()
	{
		var location = new Location(Latitude, Longitude, Timestamp)
		{
			Accuracy = Accuracy,
			Speed = Speed,
			Course = Heading,
		};

		if (Altitude is { } altitude)
		{
			location.Altitude = altitude;
			location.AltitudeReferenceSystem = AltitudeReferenceSystem.Geoid;
		}

		return location;
	}

	/// <summary>
	/// Parses the printed reply of <c>Properties.GetAll("org.freedesktop.GeoClue2.Location")</c>, e.g.
	/// <c>({'Latitude': &lt;51.5&gt;, 'Longitude': &lt;-0.1&gt;, 'Accuracy': &lt;10.0&gt;, ..., 'Timestamp': &lt;(uint64 1700000000, uint64 5)&gt;},)</c>.
	/// Returns null when latitude or longitude are missing or out of range.
	/// </summary>
	internal static GeoClueFix? Parse(string? printed)
	{
		if (string.IsNullOrEmpty(printed))
			return null;

		if (ReadDouble(printed, "Latitude") is not { } latitude || ReadDouble(printed, "Longitude") is not { } longitude)
			return null;

		if (double.IsNaN(latitude) || double.IsNaN(longitude) || Math.Abs(latitude) > 90 || Math.Abs(longitude) > 180)
			return null;

		var accuracy = ReadDouble(printed, "Accuracy");
		var altitude = ReadDouble(printed, "Altitude");
		var speed = ReadDouble(printed, "Speed");
		var heading = ReadDouble(printed, "Heading");

		return new GeoClueFix(
			latitude,
			longitude,
			accuracy is { } a && a >= 0 ? a : null,
			altitude is { } alt && alt > UnknownAltitudeBelow ? alt : null,
			speed is { } s && s >= 0 ? s : null,
			heading is { } h && h >= 0 ? h : null,
			ReadTimestamp(printed) ?? DateTimeOffset.UtcNow);
	}

	static double? ReadDouble(string printed, string name)
	{
		var m = Regex.Match(printed, "'" + name + @"'\s*:\s*<(?:double\s+)?(-?[0-9.]+(?:[eE][+-]?[0-9]+)?)>");
		return m.Success && double.TryParse(m.Groups[1].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var value) ? value : null;
	}

	static DateTimeOffset? ReadTimestamp(string printed)
	{
		var m = Regex.Match(printed, @"'Timestamp'\s*:\s*<\(\s*(?:uint64\s+)?(\d+)\s*,\s*(?:uint64\s+)?(\d+)\s*\)>");
		if (!m.Success || !long.TryParse(m.Groups[1].Value, out var seconds) || !long.TryParse(m.Groups[2].Value, out var micros))
			return null;

		try
		{
			return DateTimeOffset.FromUnixTimeSeconds(seconds).AddTicks(micros * 10);
		}
		catch (ArgumentOutOfRangeException)
		{
			return null;
		}
	}
}

/// <summary>A failed GeoClue2 call; <see cref="Unauthorized"/> when the user or the agent refused the application.</summary>
sealed class GeoClueException : Exception
{
	internal GeoClueException(string message, bool unauthorized, Exception? inner = null)
		: base(message, inner) => Unauthorized = unauthorized;

	internal bool Unauthorized { get; }
}

/// <summary>One <c>org.freedesktop.GeoClue2.Client</c>. Abstracted so the session logic can be tested without a D-Bus.</summary>
interface IGeoClueClient : IDisposable
{
	/// <summary>Raised (on any thread) for every <c>LocationUpdated</c> signal, with the new location already read.</summary>
	event EventHandler<GeoClueFix>? LocationUpdated;

	/// <summary>Sets <c>DesktopId</c>, <c>RequestedAccuracyLevel</c>, <c>TimeThreshold</c> and calls <c>Start</c>.</summary>
	/// <exception cref="GeoClueException">GeoClue refused or is unreachable.</exception>
	void Start(string desktopId, uint accuracyLevel, uint timeThresholdSeconds);

	void Stop();
}

/// <summary>The GeoClue2 manager on the system bus.</summary>
interface IGeoClueService
{
	/// <summary>The service is installed (or running) and location services are not switched off (<c>AvailableAccuracyLevel != NONE</c>).</summary>
	bool IsAvailable { get; }

	/// <exception cref="GeoClueException">GeoClue refused or is unreachable.</exception>
	IGeoClueClient CreateClient();
}

/// <summary><see cref="ILocationSource"/> over GeoClue2 (<see cref="IGeoClueService"/>).</summary>
sealed class GeoClueSource : ILocationSource
{
	/// <summary><c>GClueAccuracyLevel</c> values (<c>RequestedAccuracyLevel</c>).</summary>
	internal const uint LevelCity = 4, LevelNeighborhood = 5, LevelStreet = 6, LevelExact = 8;

	readonly IGeoClueService service;
	readonly string desktopId;

	internal GeoClueSource(IGeoClueService service, string desktopId)
	{
		this.service = service;
		this.desktopId = desktopId;
	}

	public bool IsEnabled => service.IsAvailable;

	/// <summary>GeoClue has no location until a client has started, so there is nothing cached to report.</summary>
	public Location? GetLastKnownLocation() => null;

	public ILocationSession Open(GeolocationAccuracy accuracy, TimeSpan minimumTime) =>
		new GeoClueSession(service, desktopId, ToAccuracyLevel(accuracy), ToThresholdSeconds(minimumTime));

	/// <summary>Maps the MAUI accuracy to the closest GeoClue level (city 15 km, neighborhood 1 km, street 100 m, exact).</summary>
	internal static uint ToAccuracyLevel(GeolocationAccuracy accuracy) => accuracy switch
	{
		GeolocationAccuracy.Lowest => LevelCity,
		GeolocationAccuracy.Low => LevelNeighborhood,
		GeolocationAccuracy.High or GeolocationAccuracy.Best => LevelExact,
		_ => LevelStreet,
	};

	/// <summary><c>TimeThreshold</c> is whole seconds between updates; sub-second values round up, 0 = every update.</summary>
	internal static uint ToThresholdSeconds(TimeSpan minimumTime) =>
		minimumTime <= TimeSpan.Zero ? 0u : (uint)Math.Min(uint.MaxValue, Math.Ceiling(minimumTime.TotalSeconds));
}

/// <summary>A GeoClue client that exists from <see cref="Start"/> to <see cref="Dispose"/>.</summary>
sealed class GeoClueSession : ILocationSession
{
	readonly IGeoClueService service;
	readonly string desktopId;
	readonly uint accuracyLevel;
	readonly uint thresholdSeconds;
	readonly object gate = new();
	IGeoClueClient? client;
	bool started;
	bool starting; // inside IGeoClueClient.Start: the client must not be recycled yet
	bool disposed;

	internal GeoClueSession(IGeoClueService service, string desktopId, uint accuracyLevel, uint thresholdSeconds)
	{
		this.service = service;
		this.desktopId = desktopId;
		this.accuracyLevel = accuracyLevel;
		this.thresholdSeconds = thresholdSeconds;
	}

	public event EventHandler<Location>? LocationChanged;

	public event EventHandler<GeolocationError>? Failed;

	/// <summary>Creates and starts the client off the calling thread (the D-Bus calls block); failures arrive through <see cref="Failed"/>.</summary>
	public void Start()
	{
		lock (gate)
		{
			if (disposed || started)
				return;

			started = true;
		}

		_ = Task.Run(StartClient);
	}

	void StartClient()
	{
		IGeoClueClient created;
		try
		{
			created = service.CreateClient();
		}
		catch (Exception ex)
		{
			ReportFailure(ex);
			return;
		}

		created.LocationUpdated += OnLocationUpdated;

		lock (gate)
		{
			if (disposed)
			{
				created.LocationUpdated -= OnLocationUpdated;
				created.Dispose();
				return;
			}

			client = created;
			starting = true;
		}

		Exception? error = null;
		try
		{
			created.Start(desktopId, accuracyLevel, thresholdSeconds);
		}
		catch (Exception ex)
		{
			error = ex;
		}

		bool release;
		lock (gate)
		{
			starting = false;
			release = disposed;
			if (release)
				client = null;
		}

		if (release)
		{
			// Disposed while starting: Dispose left the client to this path, so it is not recycled (and started by another session) mid-Start.
			StopAndRelease(created);
			return;
		}

		if (error is not null)
			ReportFailure(error);
	}

	void ReportFailure(Exception ex)
	{
		lock (gate)
		{
			if (disposed)
				return;
		}

		Failed?.Invoke(this, ex is GeoClueException { Unauthorized: true } ? GeolocationError.Unauthorized : GeolocationError.PositionUnavailable);
	}

	void StopAndRelease(IGeoClueClient toRelease)
	{
		toRelease.LocationUpdated -= OnLocationUpdated;
		try
		{
			toRelease.Stop();
		}
		catch (Exception)
		{
		}

		toRelease.Dispose();
	}

	void OnLocationUpdated(object? sender, GeoClueFix fix)
	{
		lock (gate)
		{
			if (disposed)
				return;
		}

		LocationChanged?.Invoke(this, fix.ToLocation());
	}

	public void Dispose()
	{
		IGeoClueClient? toDispose;
		lock (gate)
		{
			if (disposed)
				return;

			disposed = true;

			// A client still inside Start is released by StartClient once Start returns, so it is not recycled and re-started meanwhile.
			toDispose = starting ? null : client;
			client = null;
		}

		if (toDispose is null)
			return;

		_ = Task.Run(() => StopAndRelease(toDispose));
	}
}

/// <summary>
/// GeoClue2 on a bus through <c>GLib.DBusConnection</c> (blocking calls with a timeout; call off the GTK thread when it matters).
/// GtkSharp4's <c>Variant</c> has no accessors, so replies are read from their printed form.
/// </summary>
/// <remarks>
/// Every session has its own client (<c>Manager.CreateClient</c>; older GeoClue only has the per-peer <c>GetClient</c>, which sessions would share).
/// A disposed client is stopped and kept for reuse: the binding cannot build the <c>o</c> argument of <c>Manager.DeleteClient</c>,
/// so clients are recycled instead of deleted (GeoClue drops them when the process leaves the bus).
/// </remarks>
sealed class GioGeoClueService : IGeoClueService
{
	internal const string BusName = "org.freedesktop.GeoClue2";
	internal const string ManagerPath = "/org/freedesktop/GeoClue2/Manager";
	internal const string ManagerInterface = "org.freedesktop.GeoClue2.Manager";
	internal const string ClientInterface = "org.freedesktop.GeoClue2.Client";
	internal const string LocationInterface = "org.freedesktop.GeoClue2.Location";
	internal const string PropertiesInterface = "org.freedesktop.DBus.Properties";
	const int CallTimeoutMs = 3000;

	/// <summary><c>G_MAXINT</c> = no timeout: <c>Client.Start</c> is answered only after the GeoClue agent's authorization prompt, which waits for the user.</summary>
	const int NoTimeoutMs = int.MaxValue;

	readonly GLib.BusType busType;
	readonly Stack<GioGeoClueClient> idle = new();

	/// <param name="busType">The bus the service lives on; always the system bus outside tests.</param>
	internal GioGeoClueService(GLib.BusType busType = GLib.BusType.System) => this.busType = busType;

	public bool IsAvailable
	{
		get
		{
			try
			{
				var connection = Connect();
				if (!IsRunning(connection) && !IsActivatable(connection))
					return false;

				var level = Call(connection, BusName, ManagerPath, PropertiesInterface, "Get",
					GLib.Variant.NewTuple(new[] { new GLib.Variant(ManagerInterface), new GLib.Variant("AvailableAccuracyLevel") }));
				return PortalSettings.ParsePrintedUInt(level) != 0;
			}
			catch (Exception)
			{
				return false;
			}
		}
	}

	public IGeoClueClient CreateClient()
	{
		lock (idle)
		{
			if (idle.Count > 0)
				return idle.Pop();
		}

		try
		{
			var connection = Connect();
			var path = ParseObjectPath(CreateClientPath(connection));
			return path is null
				? throw new GeoClueException("GeoClue returned no client.", false)
				: new GioGeoClueClient(this, connection, path);
		}
		catch (GeoClueException)
		{
			throw;
		}
		catch (Exception ex)
		{
			throw Wrap(ex);
		}
	}

	/// <summary>
	/// <c>Manager.CreateClient</c> (GeoClue 2.5+) returns a new client per call; <c>GetClient</c> returns the one client of this bus peer on every call,
	/// which sessions would share (one stopping it stops all, each overwriting the others' accuracy), so it is only the fallback for older GeoClue.
	/// </summary>
	static string? CreateClientPath(GLib.DBusConnection connection)
	{
		try
		{
			return Call(connection, BusName, ManagerPath, ManagerInterface, "CreateClient", null);
		}
		catch (Exception ex) when (IsUnknownMethod(ex))
		{
			return Call(connection, BusName, ManagerPath, ManagerInterface, "GetClient", null);
		}
	}

	internal static bool IsUnknownMethod(Exception ex) =>
		ex.Message.Contains("UnknownMethod", StringComparison.OrdinalIgnoreCase)
		|| ex.Message.Contains("No such method", StringComparison.OrdinalIgnoreCase);

	internal void Recycle(GioGeoClueClient client)
	{
		lock (idle)
			idle.Push(client);
	}

	GLib.DBusConnection Connect() =>
		GLib.Bus.GetSync(busType, null!) ?? throw new GeoClueException("No D-Bus connection.", false);

	static bool IsRunning(GLib.DBusConnection connection) =>
		Call(connection, "org.freedesktop.DBus", "/org/freedesktop/DBus", "org.freedesktop.DBus", "NameHasOwner",
			GLib.Variant.NewTuple(new[] { new GLib.Variant(BusName) }))?.IndexOf("true", StringComparison.Ordinal) >= 0;

	static bool IsActivatable(GLib.DBusConnection connection) =>
		Call(connection, "org.freedesktop.DBus", "/org/freedesktop/DBus", "org.freedesktop.DBus", "ListActivatableNames", null)
			?.Contains("'" + BusName + "'", StringComparison.Ordinal) == true;

	internal static string? Call(GLib.DBusConnection connection, string name, string path, string iface, string method, GLib.Variant? args, bool waitForUser = false)
	{
		try
		{
			using var reply = connection.CallSync(name, path, iface, method, args!, null!, GLib.DBusCallFlags.AllowInteractiveAuthorization, waitForUser ? NoTimeoutMs : CallTimeoutMs, null!);
			return reply?.Print(true);
		}
		finally
		{
			args?.Dispose();
		}
	}

	/// <summary>The object path of a printed <c>(objectpath '/a/b',)</c> reply; for a signal's <c>(old, new)</c> pair, the last path.</summary>
	internal static string? ParseObjectPath(string? printed)
	{
		if (string.IsNullOrEmpty(printed))
			return null;

		var matches = Regex.Matches(printed, "'(/[^']*)'");
		return matches.Count == 0 ? null : matches[matches.Count - 1].Groups[1].Value;
	}

	/// <summary>Maps a failed call to <see cref="GeoClueException"/>; an access-denied D-Bus error means the user refused the app.</summary>
	internal static GeoClueException Wrap(Exception ex)
	{
		var text = ex.Message;
		var unauthorized = text.Contains("AccessDenied", StringComparison.OrdinalIgnoreCase)
			|| text.Contains("not authorized", StringComparison.OrdinalIgnoreCase)
			|| text.Contains("not authorised", StringComparison.OrdinalIgnoreCase)
			|| text.Contains("Permission denied", StringComparison.OrdinalIgnoreCase);
		return new GeoClueException(text, unauthorized, ex);
	}
}

/// <summary>A live <c>org.freedesktop.GeoClue2.Client</c> object; disposing returns it to its service for reuse.</summary>
sealed class GioGeoClueClient : IGeoClueClient
{
	readonly GioGeoClueService owner;
	readonly GLib.DBusConnection connection;
	readonly string path;
	readonly GLib.DBusSignalCallback callback; // kept reachable: native code holds only a function pointer

	internal GioGeoClueClient(GioGeoClueService owner, GLib.DBusConnection connection, string path)
	{
		this.owner = owner;
		this.connection = connection;
		this.path = path;
		callback = OnSignal;
		connection.SignalSubscribe(GioGeoClueService.BusName, GioGeoClueService.ClientInterface, "LocationUpdated",
			path, null!, GLib.DBusSignalFlags.None, callback);
	}

	public event EventHandler<GeoClueFix>? LocationUpdated;

	public void Start(string desktopId, uint accuracyLevel, uint timeThresholdSeconds)
	{
		try
		{
			SetProperty("DesktopId", new GLib.Variant(desktopId));
			SetProperty("RequestedAccuracyLevel", new GLib.Variant(accuracyLevel));
			SetProperty("TimeThreshold", new GLib.Variant(timeThresholdSeconds));
			GioGeoClueService.Call(connection, GioGeoClueService.BusName, path, GioGeoClueService.ClientInterface, "Start", null, waitForUser: true);
		}
		catch (Exception ex)
		{
			throw GioGeoClueService.Wrap(ex);
		}
	}

	public void Stop() =>
		GioGeoClueService.Call(connection, GioGeoClueService.BusName, path, GioGeoClueService.ClientInterface, "Stop", null);

	void SetProperty(string name, GLib.Variant value) =>
		GioGeoClueService.Call(connection, GioGeoClueService.BusName, path, GioGeoClueService.PropertiesInterface, "Set",
			GLib.Variant.NewTuple(new[] { new GLib.Variant(GioGeoClueService.ClientInterface), new GLib.Variant(name), GLib.Variant.NewVariant(value) }));

	void OnSignal(GLib.DBusConnection source, string senderName, string objectPath, string interfaceName, string signalName, GLib.Variant parameters)
	{
		if (LocationUpdated is null)
			return;

		string? locationPath;
		try
		{
			locationPath = GioGeoClueService.ParseObjectPath(parameters?.Print(true));
		}
		catch (Exception)
		{
			return;
		}

		if (locationPath is null or "/")
			return;

		// the signal arrives on the GTK main loop: read the location (a blocking call) on a worker
		_ = Task.Run(() => ReadAndRaise(locationPath));
	}

	void ReadAndRaise(string locationPath)
	{
		try
		{
			var printed = GioGeoClueService.Call(connection, GioGeoClueService.BusName, locationPath, GioGeoClueService.PropertiesInterface, "GetAll",
				GLib.Variant.NewTuple(new[] { new GLib.Variant(GioGeoClueService.LocationInterface) }));

			if (GeoClueFix.Parse(printed) is { } fix)
				LocationUpdated?.Invoke(this, fix);
		}
		catch (Exception)
		{
			// a failed read only skips this update
		}
	}

	public void Dispose() => owner.Recycle(this);
}
