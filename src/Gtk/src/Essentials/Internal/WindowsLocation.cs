using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Maui.Devices.Sensors;

namespace Microsoft.Maui.Essentials.Gtk;

/// <summary><c>ReportStatus</c> of the Windows Location API (<c>locationapi.h</c>).</summary>
enum LocationReportStatus
{
	NotSupported = 0,
	Error = 1,
	AccessDenied = 2,
	Initializing = 3,
	Running = 4,
}

/// <summary>One Windows Location API report factory (a stream of lat/long reports). Abstracted so the polling logic can be tested.</summary>
interface ILocationReports : IDisposable
{
	LocationReportStatus Status { get; }

	/// <summary>Asks the service to produce reports at most every <paramref name="intervalMs"/> milliseconds.</summary>
	void Listen(int intervalMs);

	void StopListening();

	/// <summary>The latest report, or null while there is none.</summary>
	Location? ReadReport();
}

/// <summary>
/// <see cref="ILocationSource"/> over a polled <see cref="ILocationReports"/>: the Windows Location API is a COM automation
/// object without managed event support, so each session reads its report on a timer and raises a fix when the report is new.
/// </summary>
sealed class PollingLocationSource : ILocationSource
{
	readonly Func<ILocationReports> create;
	readonly TimeSpan minimumPoll;

	/// <param name="create">Creates a report factory; one per session, plus short-lived ones for probes.</param>
	/// <param name="minimumPoll">Lower bound of the poll interval (default 500 ms).</param>
	internal PollingLocationSource(Func<ILocationReports> create, TimeSpan? minimumPoll = null)
	{
		this.create = create;
		this.minimumPoll = minimumPoll ?? TimeSpan.FromMilliseconds(500);
	}

	/// <summary>True when the service exists and reports no error. Windows also reports "access denied" while location is switched off, so that reads as disabled.</summary>
	public bool IsEnabled
	{
		get
		{
			using var reports = create();
			reports.Listen(1000);
			try
			{
				return reports.Status is LocationReportStatus.Running or LocationReportStatus.Initializing;
			}
			finally
			{
				reports.StopListening();
			}
		}
	}

	/// <summary>Best effort: the report the service already holds, if any.</summary>
	public Location? GetLastKnownLocation()
	{
		using var reports = create();
		return reports.ReadReport();
	}

	public ILocationSession Open(GeolocationAccuracy accuracy, TimeSpan minimumTime) =>
		new PollingSession(create, minimumTime > minimumPoll ? minimumTime : minimumPoll);

	sealed class PollingSession : ILocationSession
	{
		readonly Func<ILocationReports> create;
		readonly TimeSpan interval;
		readonly object gate = new();
		ILocationReports? reports;
		Timer? timer;
		DateTimeOffset? lastTimestamp;
		bool started;
		bool disposed;

		internal PollingSession(Func<ILocationReports> create, TimeSpan interval)
		{
			this.create = create;
			this.interval = interval;
		}

		public event EventHandler<Location>? LocationChanged;

		public event EventHandler<GeolocationError>? Failed;

		public void Start()
		{
			lock (gate)
			{
				if (disposed || started)
					return;

				started = true;
				timer = new Timer(_ => Poll(), null, TimeSpan.Zero, interval);
			}
		}

		void Poll()
		{
			GeolocationError? failure = null;
			Location? fix = null;

			lock (gate)
			{
				if (disposed)
					return;

				try
				{
					if (reports is null)
					{
						reports = create();
						reports.Listen((int)Math.Min(int.MaxValue, interval.TotalMilliseconds));
					}

					switch (reports.Status)
					{
						case LocationReportStatus.AccessDenied:
							failure = GeolocationError.Unauthorized;
							break;
						case LocationReportStatus.NotSupported:
						case LocationReportStatus.Error:
							failure = GeolocationError.PositionUnavailable;
							break;
						case LocationReportStatus.Running:
							var report = reports.ReadReport();
							if (report is not null && report.Timestamp != lastTimestamp)
							{
								lastTimestamp = report.Timestamp;
								fix = report;
							}

							break;
					}
				}
				catch (Exception)
				{
					failure = GeolocationError.PositionUnavailable;
				}
			}

			if (failure is { } error)
			{
				Dispose();
				Failed?.Invoke(this, error);
			}
			else if (fix is not null)
			{
				LocationChanged?.Invoke(this, fix);
			}
		}

		public void Dispose()
		{
			lock (gate)
			{
				if (disposed)
					return;

				disposed = true;
				timer?.Dispose();
				timer = null;

				if (reports is not null)
				{
					try
					{
						reports.StopListening();
					}
					catch (Exception)
					{
					}

					reports.Dispose();
					reports = null;
				}
			}
		}
	}
}

/// <summary>
/// The Windows Location API through its automation objects (<c>LocationDisp.LatLongReportFactory</c> / <c>DispLatLongReport</c>),
/// late bound with <see cref="Type.InvokeMember(string, BindingFlags, Binder, object, object[])"/> so no COM interop assembly or WinRT
/// projection is needed from plain <c>net10.0</c>. Needs the built-in COM interop (not available when trimmed or AOT-compiled).
/// </summary>
sealed class ComLocationReports : ILocationReports
{
	const string FactoryProgId = "LocationDisp.LatLongReportFactory";

	object? factory;

	internal ComLocationReports()
	{
		try
		{
			if (OperatingSystem.IsWindows() && Type.GetTypeFromProgID(FactoryProgId) is { } type)
				factory = Activator.CreateInstance(type);
		}
		catch (Exception)
		{
			factory = null;
		}
	}

	public LocationReportStatus Status
	{
		get
		{
			if (factory is null)
				return LocationReportStatus.NotSupported;

			try
			{
				return (LocationReportStatus)Convert.ToInt32(Get(factory, "Status"));
			}
			catch (Exception)
			{
				return LocationReportStatus.Error;
			}
		}
	}

	public void Listen(int intervalMs)
	{
		if (factory is not null)
			Invoke(factory, "ListenForReports", BindingFlags.InvokeMethod, (uint)Math.Max(1, intervalMs));
	}

	public void StopListening()
	{
		if (factory is not null)
			Invoke(factory, "StopListeningForReports", BindingFlags.InvokeMethod);
	}

	public Location? ReadReport()
	{
		if (factory is null)
			return null;

		object? report = null;
		try
		{
			report = Get(factory, "LatLongReport");
			if (report is null)
				return null;

			var latitude = ToDouble(Get(report, "Latitude"));
			var longitude = ToDouble(Get(report, "Longitude"));
			if (latitude is null or double.NaN || longitude is null or double.NaN)
				return null;

			return Create(latitude.Value, longitude.Value, ToDouble(Get(report, "ErrorRadius")),
				ToDouble(Get(report, "Altitude")), ToDouble(Get(report, "AltitudeError")), Get(report, "Timestamp"));
		}
		catch (Exception)
		{
			return null;
		}
		finally
		{
			if (report is not null && Marshal.IsComObject(report))
				Marshal.FinalReleaseComObject(report);
		}
	}

	/// <summary>Builds the MAUI location from the report values; NaN / non-positive means "unknown" for the radius and altitude.</summary>
	internal static Location? Create(double latitude, double longitude, double? errorRadius, double? altitude, double? altitudeError, object? timestamp)
	{
		if (Math.Abs(latitude) > 90 || Math.Abs(longitude) > 180)
			return null;

		var time = timestamp is DateTime dt
			? new DateTimeOffset(DateTime.SpecifyKind(dt, DateTimeKind.Utc))
			: DateTimeOffset.UtcNow;

		var location = new Location(latitude, longitude, time);
		if (errorRadius is { } radius && !double.IsNaN(radius) && radius >= 0)
			location.Accuracy = radius;

		if (altitude is { } alt && !double.IsNaN(alt))
		{
			location.Altitude = alt;
			location.AltitudeReferenceSystem = AltitudeReferenceSystem.Geoid;
			if (altitudeError is { } error && !double.IsNaN(error) && error >= 0)
				location.VerticalAccuracy = error;
		}

		return location;
	}

	public void Dispose()
	{
		var current = factory;
		factory = null;
		if (current is not null && Marshal.IsComObject(current))
			Marshal.FinalReleaseComObject(current);
	}

	static object? Get(object target, string property) =>
		Invoke(target, property, BindingFlags.GetProperty);

	/// <summary>IDispatch late binding: the members are resolved by the COM object, not by managed reflection metadata, so trimming cannot remove them.</summary>
	[UnconditionalSuppressMessage("Trimming", "IL2075", Justification = "Invokes members of a COM automation object through IDispatch.")]
	static object? Invoke(object target, string member, BindingFlags flags, params object[] args) =>
		target.GetType().InvokeMember(member, flags, null, target, args.Length == 0 ? null : args);

	static double? ToDouble(object? value) =>
		value is null ? null : Convert.ToDouble(value, System.Globalization.CultureInfo.InvariantCulture);
}
