using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Devices.Sensors;

namespace Microsoft.Maui.Essentials.Gtk;

/// <summary>
/// <see cref="IGeolocation"/> for Linux (GeoClue2 over D-Bus) and Windows (Location API), through an <see cref="ILocationSource"/>.
/// </summary>
/// <remarks>
/// <para><see cref="GetLocationAsync"/> returns the first fix, or <see langword="null"/> when <see cref="GeolocationRequest.Timeout"/>
/// (when positive) elapses first; cancelling the token throws <see cref="OperationCanceledException"/>.
/// <see cref="LocationChanged"/> and <see cref="ListeningFailed"/> are raised on the GTK main thread.</para>
/// <para>Each request opens its own session (its own GeoClue client on Linux), so listening and one-shot requests do not disturb each other.
/// <see cref="GetLocationAsync"/> and <see cref="StartListeningForegroundAsync"/> probe <see cref="IsEnabled"/> off the calling thread.</para>
/// </remarks>
sealed class GtkGeolocation : IGeolocation
{
	readonly ILocationSource source;
	readonly object gate = new();
	Location? lastKnown;
	ILocationSession? listening;

	internal GtkGeolocation(string applicationId, ILocationSource? source = null) =>
		this.source = source ?? LocationSources.CreateDefault(applicationId);

	public event EventHandler<GeolocationLocationChangedEventArgs>? LocationChanged;

	public event EventHandler<GeolocationListeningFailedEventArgs>? ListeningFailed;

	public bool IsListeningForeground
	{
		get
		{
			lock (gate)
				return listening is not null;
		}
	}

	public bool IsEnabled => SafeIsEnabled();

	public Task<Location?> GetLastKnownLocationAsync()
	{
		Location? cached;
		lock (gate)
			cached = lastKnown;

		if (cached is not null)
			return Task.FromResult<Location?>(cached);

		try
		{
			return Task.FromResult(source.GetLastKnownLocation());
		}
		catch (Exception)
		{
			return Task.FromResult<Location?>(null);
		}
	}

	public async Task<Location?> GetLocationAsync(GeolocationRequest request, CancellationToken cancelToken)
	{
		ArgumentNullException.ThrowIfNull(request);
		cancelToken.ThrowIfCancellationRequested();

		// the probe makes blocking system-bus calls: keep them off the caller's thread (often the GTK main thread)
		if (!await Task.Run(SafeIsEnabled).ConfigureAwait(false))
			throw new FeatureNotEnabledException("Location services are not enabled on device.");

		cancelToken.ThrowIfCancellationRequested();

		var tcs = new TaskCompletionSource<Location?>(TaskCreationOptions.RunContinuationsAsynchronously);
		using var session = source.Open(request.DesiredAccuracy, TimeSpan.Zero);

		session.LocationChanged += (_, location) =>
		{
			Remember(location);
			tcs.TrySetResult(location);
		};
		session.Failed += (_, error) => tcs.TrySetException(ToException(error));

		using var timeout = new CancellationTokenSource();
		if (request.Timeout > TimeSpan.Zero)
			timeout.CancelAfter(request.Timeout);

		using var timeoutRegistration = timeout.Token.Register(() => tcs.TrySetResult(null));
		using var cancelRegistration = cancelToken.Register(() => tcs.TrySetCanceled(cancelToken));

		session.Start();
		return await tcs.Task.ConfigureAwait(false);
	}

	public Task<bool> StartListeningForegroundAsync(GeolocationListeningRequest request)
	{
		ArgumentNullException.ThrowIfNull(request);

		if (request.MinimumTime < TimeSpan.Zero)
			throw new ArgumentOutOfRangeException(nameof(request), "MinimumTime must be positive.");

		lock (gate)
		{
			if (listening is not null)
				throw new InvalidOperationException("Already listening to location updates.");
		}

		return StartListeningCoreAsync(request);
	}

	async Task<bool> StartListeningCoreAsync(GeolocationListeningRequest request)
	{
		// the probe may block on D-Bus/COM: keep it off the caller's thread (often the GTK main thread), as GetLocationAsync does
		if (!await Task.Run(SafeIsEnabled).ConfigureAwait(false))
			return false;

		ILocationSession session;
		try
		{
			session = source.Open(request.DesiredAccuracy, request.MinimumTime);
		}
		catch (Exception)
		{
			return false;
		}

		lock (gate)
		{
			if (listening is not null)
			{
				session.Dispose();
				throw new InvalidOperationException("Already listening to location updates.");
			}

			listening = session;
		}

		session.LocationChanged += (_, location) => OnListeningLocation(session, location);
		session.Failed += (_, error) => OnListeningFailed(session, error);

		try
		{
			session.Start();
		}
		catch (Exception)
		{
			Detach(session);
			return false;
		}

		return true;
	}

	public void StopListeningForeground()
	{
		ILocationSession? session;
		lock (gate)
		{
			session = listening;
			listening = null;
		}

		session?.Dispose();
	}

	bool SafeIsEnabled()
	{
		try
		{
			return source.IsEnabled;
		}
		catch (Exception)
		{
			return false;
		}
	}

	void Remember(Location location)
	{
		lock (gate)
			lastKnown = location;
	}

	/// <summary>Disposes <paramref name="session"/> when it is still the active listener; returns whether it was.</summary>
	bool Detach(ILocationSession session)
	{
		lock (gate)
		{
			if (!ReferenceEquals(listening, session))
				return false;

			listening = null;
		}

		session.Dispose();
		return true;
	}

	void OnListeningLocation(ILocationSession session, Location location)
	{
		lock (gate)
		{
			if (!ReferenceEquals(listening, session))
				return;

			lastKnown = location;
		}

		var handler = LocationChanged;
		if (handler is not null)
			_ = GtkDispatch.InvokeAsync(() => handler(this, new GeolocationLocationChangedEventArgs(location)));
	}

	void OnListeningFailed(ILocationSession session, GeolocationError error)
	{
		if (!Detach(session))
			return;

		var handler = ListeningFailed;
		if (handler is not null)
			_ = GtkDispatch.InvokeAsync(() => handler(this, new GeolocationListeningFailedEventArgs(error)));
	}

	static Exception ToException(GeolocationError error) => error == GeolocationError.Unauthorized
		? new PermissionException("Location permission was denied.")
		: new FeatureNotEnabledException("Location is not available on device.");
}

/// <summary>Chooses the <see cref="ILocationSource"/> of the running OS.</summary>
static class LocationSources
{
	internal static ILocationSource CreateDefault(string applicationId) =>
		OperatingSystem.IsWindows()
			? new PollingLocationSource(() => new ComLocationReports())
			: new GeoClueSource(new GioGeoClueService(), applicationId);
}
