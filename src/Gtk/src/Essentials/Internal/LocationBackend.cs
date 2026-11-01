using System;
using Microsoft.Maui.Devices.Sensors;

namespace Microsoft.Maui.Essentials.Gtk;

/// <summary>
/// A platform location provider (GeoClue2 on Linux, the Windows Location API on Windows). Abstracted so
/// <see cref="GtkGeolocation"/> can be tested against a fake.
/// </summary>
interface ILocationSource
{
	/// <summary>Whether the location service exists and the user has not switched it off.</summary>
	bool IsEnabled { get; }

	/// <summary>A cached location the platform can report without waiting for a fix, or null.</summary>
	Location? GetLastKnownLocation();

	/// <summary>Creates a stopped session; subscribe to its events, then call <see cref="ILocationSession.Start"/>.</summary>
	ILocationSession Open(GeolocationAccuracy accuracy, TimeSpan minimumTime);
}

/// <summary>One stream of location fixes. Events may be raised on any thread.</summary>
interface ILocationSession : IDisposable
{
	event EventHandler<Location>? LocationChanged;

	/// <summary>The session cannot deliver fixes (and has stopped).</summary>
	event EventHandler<GeolocationError>? Failed;

	/// <summary>Begins delivering fixes. Start-up problems after this call returns are reported through <see cref="Failed"/>.</summary>
	void Start();
}
