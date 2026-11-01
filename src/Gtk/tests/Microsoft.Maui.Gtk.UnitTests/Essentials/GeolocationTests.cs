using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.Essentials.Gtk;
using Xunit;

namespace Microsoft.Maui.Gtk.UnitTests.Essentials;

sealed class FakeLocationSession : ILocationSession
{
	public event EventHandler<Location>? LocationChanged;

	public event EventHandler<GeolocationError>? Failed;

	public bool Started { get; private set; }

	public bool Disposed { get; private set; }

	public GeolocationAccuracy Accuracy { get; init; }

	public TimeSpan MinimumTime { get; init; }

	public Exception? StartException { get; set; }

	public void Start()
	{
		if (StartException is not null)
			throw StartException;

		Started = true;
	}

	public void Dispose() => Disposed = true;

	public void Raise(Location location) => LocationChanged?.Invoke(this, location);

	public void Fail(GeolocationError error) => Failed?.Invoke(this, error);
}

sealed class FakeLocationSource : ILocationSource
{
	public bool IsEnabled { get; set; } = true;

	public Location? LastKnown { get; set; }

	public List<FakeLocationSession> Sessions { get; } = new();

	public Location? GetLastKnownLocation() => LastKnown;

	public ILocationSession Open(GeolocationAccuracy accuracy, TimeSpan minimumTime)
	{
		var session = new FakeLocationSession { Accuracy = accuracy, MinimumTime = minimumTime };
		Sessions.Add(session);
		return session;
	}
}

// Same collection as the GTK tests: LocationChanged/ListeningFailed are marshalled to the GTK thread, which must not be busy with a parallel test.
[Collection("Gtk")]
public class GtkGeolocationTests
{
	static Location Fix(double lat = 10, double lon = 20) => new(lat, lon, DateTimeOffset.UtcNow);

	static GtkGeolocation Create(ILocationSource source) => new("org.maui.gtk.tests", source);

	[Fact]
	public void IsEnabledFollowsTheSource()
	{
		var source = new FakeLocationSource { IsEnabled = false };
		var geo = Create(source);

		Assert.False(geo.IsEnabled);
		source.IsEnabled = true;
		Assert.True(geo.IsEnabled);
	}

	[Fact]
	public async Task GetLocationReturnsTheFirstFixAndClosesItsSession()
	{
		var source = new FakeLocationSource();
		var geo = Create(source);

		var task = geo.GetLocationAsync(new GeolocationRequest(GeolocationAccuracy.High, TimeSpan.FromSeconds(30)), default);
		var session = Assert.Single(source.Sessions);
		Assert.True(session.Started);
		Assert.Equal(GeolocationAccuracy.High, session.Accuracy);
		Assert.Equal(TimeSpan.Zero, session.MinimumTime);

		session.Raise(Fix(1, 2));
		session.Raise(Fix(3, 4));

		var location = await task;
		Assert.Equal(1, location!.Latitude);
		Assert.Equal(2, location.Longitude);
		Assert.True(session.Disposed);
	}

	[Theory]
	[InlineData(GeolocationAccuracy.Lowest)]
	[InlineData(GeolocationAccuracy.Default)]
	[InlineData(GeolocationAccuracy.Best)]
	public async Task GetLocationPassesTheRequestedAccuracy(GeolocationAccuracy accuracy)
	{
		var source = new FakeLocationSource();
		var task = Create(source).GetLocationAsync(new GeolocationRequest(accuracy, TimeSpan.FromSeconds(30)), default);

		var session = Assert.Single(source.Sessions);
		Assert.Equal(accuracy, session.Accuracy);
		session.Raise(Fix());
		await task;
	}

	[Theory]
	[InlineData(50)]
	[InlineData(150)]
	public async Task GetLocationReturnsNullWhenTheTimeoutElapses(int timeoutMs)
	{
		var source = new FakeLocationSource();
		var geo = Create(source);

		var location = await geo.GetLocationAsync(new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromMilliseconds(timeoutMs)), default)
			.WaitAsync(TimeSpan.FromSeconds(10));

		Assert.Null(location);
		Assert.True(Assert.Single(source.Sessions).Disposed);
	}

	[Fact]
	public async Task GetLocationWithoutTimeoutWaitsForTheFix()
	{
		var source = new FakeLocationSource();
		var task = Create(source).GetLocationAsync(new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.Zero), default);

		await Task.Delay(150);
		Assert.False(task.IsCompleted);

		source.Sessions[0].Raise(Fix());
		Assert.NotNull(await task.WaitAsync(TimeSpan.FromSeconds(10)));
	}

	[Fact]
	public async Task GetLocationCancellationThrowsAndClosesTheSession()
	{
		var source = new FakeLocationSource();
		using var cts = new CancellationTokenSource();
		var task = Create(source).GetLocationAsync(new GeolocationRequest(), cts.Token);

		cts.Cancel();

		await Assert.ThrowsAnyAsync<OperationCanceledException>(() => task);
		Assert.True(Assert.Single(source.Sessions).Disposed);
	}

	[Fact]
	public async Task GetLocationWithAnAlreadyCancelledTokenDoesNotOpenASession()
	{
		var source = new FakeLocationSource();
		using var cts = new CancellationTokenSource();
		cts.Cancel();

		await Assert.ThrowsAnyAsync<OperationCanceledException>(() => Create(source).GetLocationAsync(new GeolocationRequest(), cts.Token));
		Assert.Empty(source.Sessions);
	}

	[Fact]
	public async Task GetLocationValidatesAndRequiresEnabledServices()
	{
		var source = new FakeLocationSource();
		var geo = Create(source);

		await Assert.ThrowsAsync<ArgumentNullException>(() => geo.GetLocationAsync(null!, default));

		source.IsEnabled = false;
		await Assert.ThrowsAsync<FeatureNotEnabledException>(() => geo.GetLocationAsync(new GeolocationRequest(), default));
		Assert.Empty(source.Sessions);
	}

	[Theory]
	[InlineData(GeolocationError.Unauthorized, typeof(PermissionException))]
	[InlineData(GeolocationError.PositionUnavailable, typeof(FeatureNotEnabledException))]
	public async Task GetLocationMapsSessionFailuresToExceptions(GeolocationError error, Type exception)
	{
		var source = new FakeLocationSource();
		var task = Create(source).GetLocationAsync(new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(30)), default);

		source.Sessions[0].Fail(error);

		var thrown = await Assert.ThrowsAnyAsync<Exception>(() => task);
		Assert.IsType(exception, thrown);
		Assert.True(source.Sessions[0].Disposed);
	}

	[Fact]
	public async Task LastKnownIsTheSourceValueThenTheLatestFix()
	{
		var source = new FakeLocationSource { LastKnown = Fix(5, 6) };
		var geo = Create(source);

		Assert.Equal(5, (await geo.GetLastKnownLocationAsync())!.Latitude);

		var task = geo.GetLocationAsync(new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(30)), default);
		source.Sessions[0].Raise(Fix(7, 8));
		await task;

		Assert.Equal(7, (await geo.GetLastKnownLocationAsync())!.Latitude);
	}

	[Fact]
	public async Task LastKnownIsNullWithoutAnyFix()
	{
		Assert.Null(await Create(new FakeLocationSource()).GetLastKnownLocationAsync());
	}

	[Fact]
	public async Task ListeningStartsASessionWithTheRequestAndRaisesLocationChanged()
	{
		var source = new FakeLocationSource();
		var geo = Create(source);
		var received = new TaskCompletionSource<Location>();
		geo.LocationChanged += (_, e) => received.TrySetResult(e.Location);

		Assert.False(geo.IsListeningForeground);
		Assert.True(await geo.StartListeningForegroundAsync(new GeolocationListeningRequest(GeolocationAccuracy.Best, TimeSpan.FromSeconds(3))));

		var session = Assert.Single(source.Sessions);
		Assert.True(geo.IsListeningForeground);
		Assert.True(session.Started);
		Assert.Equal(GeolocationAccuracy.Best, session.Accuracy);
		Assert.Equal(TimeSpan.FromSeconds(3), session.MinimumTime);

		session.Raise(Fix(11, 12));

		var location = await received.Task.WaitAsync(TimeSpan.FromSeconds(10));
		Assert.Equal(11, location.Latitude);
		Assert.Equal(11, (await geo.GetLastKnownLocationAsync())!.Latitude);
	}

	[Fact]
	public async Task StartingTwiceThrowsAndStopClosesTheSession()
	{
		var source = new FakeLocationSource();
		var geo = Create(source);
		await geo.StartListeningForegroundAsync(new GeolocationListeningRequest());

		await Assert.ThrowsAsync<InvalidOperationException>(() => geo.StartListeningForegroundAsync(new GeolocationListeningRequest()));

		geo.StopListeningForeground();
		Assert.False(geo.IsListeningForeground);
		Assert.True(source.Sessions[0].Disposed);

		geo.StopListeningForeground(); // no effect

		Assert.True(await geo.StartListeningForegroundAsync(new GeolocationListeningRequest()));
		Assert.Equal(2, source.Sessions.Count);
	}

	[Fact]
	public async Task NoEventsAreRaisedAfterStop()
	{
		var source = new FakeLocationSource();
		var geo = Create(source);
		var count = 0;
		geo.LocationChanged += (_, _) => Interlocked.Increment(ref count);
		await geo.StartListeningForegroundAsync(new GeolocationListeningRequest());
		var session = source.Sessions[0];

		geo.StopListeningForeground();
		session.Raise(Fix());
		await Task.Delay(100);

		Assert.Equal(0, count);
	}

	[Fact]
	public async Task ListeningValidatesTheRequest()
	{
		var geo = Create(new FakeLocationSource());

		await Assert.ThrowsAsync<ArgumentNullException>(() => geo.StartListeningForegroundAsync(null!));
		await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
			geo.StartListeningForegroundAsync(new GeolocationListeningRequest(GeolocationAccuracy.Best, TimeSpan.FromSeconds(-1))));
	}

	[Fact]
	public async Task ListeningReturnsFalseWhenDisabledOrTheSessionCannotStart()
	{
		var source = new FakeLocationSource { IsEnabled = false };
		var geo = Create(source);

		Assert.False(await geo.StartListeningForegroundAsync(new GeolocationListeningRequest()));
		Assert.False(geo.IsListeningForeground);

		var throwing = new ThrowingStartSource();
		var geo3 = Create(throwing);
		Assert.False(await geo3.StartListeningForegroundAsync(new GeolocationListeningRequest()));
		Assert.False(geo3.IsListeningForeground);
		Assert.True(throwing.Session.Disposed);
	}

	sealed class ThrowingStartSource : ILocationSource
	{
		public FakeLocationSession Session { get; } = new() { StartException = new InvalidOperationException("no") };

		public bool IsEnabled => true;

		public Location? GetLastKnownLocation() => null;

		public ILocationSession Open(GeolocationAccuracy accuracy, TimeSpan minimumTime) => Session;
	}

	[Theory]
	[InlineData(GeolocationError.Unauthorized)]
	[InlineData(GeolocationError.PositionUnavailable)]
	public async Task ListeningFailureStopsListeningAndRaisesListeningFailed(GeolocationError error)
	{
		var source = new FakeLocationSource();
		var geo = Create(source);
		var failed = new TaskCompletionSource<GeolocationError>();
		geo.ListeningFailed += (_, e) => failed.TrySetResult(e.Error);
		await geo.StartListeningForegroundAsync(new GeolocationListeningRequest());

		source.Sessions[0].Fail(error);

		Assert.Equal(error, await failed.Task.WaitAsync(TimeSpan.FromSeconds(10)));
		Assert.False(geo.IsListeningForeground);
		Assert.True(source.Sessions[0].Disposed);
	}

	[Fact]
	public async Task ListeningAndOneShotRequestsUseSeparateSessions()
	{
		var source = new FakeLocationSource();
		var geo = Create(source);
		var updates = 0;
		geo.LocationChanged += (_, _) => Interlocked.Increment(ref updates);
		await geo.StartListeningForegroundAsync(new GeolocationListeningRequest());

		var one = geo.GetLocationAsync(new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(30)), default);
		Assert.Equal(2, source.Sessions.Count);
		source.Sessions[1].Raise(Fix());
		await one;

		Assert.True(geo.IsListeningForeground);
		Assert.False(source.Sessions[0].Disposed);
		Assert.True(source.Sessions[1].Disposed);
		await Task.Delay(100);
		Assert.Equal(0, updates);
	}
}

public class GeoClueTests
{
	const string FullReply = "({'Latitude': <51.5074>, 'Longitude': <-0.1278>, 'Accuracy': <25.0>, 'Altitude': <35.5>, 'Speed': <3.5>, "
		+ "'Heading': <90.0>, 'Description': <'London'>, 'Timestamp': <(uint64 1700000000, uint64 500000)>},)";

	[Fact]
	public void ParseReadsEveryField()
	{
		var fix = GeoClueFix.Parse(FullReply)!;

		Assert.Equal(51.5074, fix.Latitude);
		Assert.Equal(-0.1278, fix.Longitude);
		Assert.Equal(25.0, fix.Accuracy);
		Assert.Equal(35.5, fix.Altitude);
		Assert.Equal(3.5, fix.Speed);
		Assert.Equal(90.0, fix.Heading);
		Assert.Equal(DateTimeOffset.FromUnixTimeSeconds(1700000000).AddTicks(5_000_000), fix.Timestamp);
	}

	[Fact]
	public void ParseTurnsGeoClueUnknownSentinelsIntoNull()
	{
		var fix = GeoClueFix.Parse("({'Latitude': <1.0>, 'Longitude': <2.0>, 'Accuracy': <-1.0>, 'Altitude': <-1.7976931348623157e+308>, "
			+ "'Speed': <-1.0>, 'Heading': <-1.0>, 'Description': <''>, 'Timestamp': <(1700000000, 0)>},)")!;

		Assert.Null(fix.Accuracy);
		Assert.Null(fix.Altitude);
		Assert.Null(fix.Speed);
		Assert.Null(fix.Heading);

		var location = fix.ToLocation();
		Assert.Null(location.Altitude);
		Assert.Null(location.Accuracy);
		Assert.Null(location.Speed);
		Assert.Null(location.Course);
		Assert.Equal(DateTimeOffset.FromUnixTimeSeconds(1700000000), location.Timestamp);
	}

	[Theory]
	[InlineData(null)]
	[InlineData("")]
	[InlineData("({'Longitude': <2.0>},)")]
	[InlineData("({'Latitude': <1.0>},)")]
	[InlineData("({'Latitude': <91.0>, 'Longitude': <2.0>},)")]
	[InlineData("({'Latitude': <1.0>, 'Longitude': <-181.0>},)")]
	public void ParseRejectsMissingOrOutOfRangeCoordinates(string? printed)
	{
		Assert.Null(GeoClueFix.Parse(printed));
	}

	[Fact]
	public void ToLocationMapsAllMembersWithAGeoidAltitude()
	{
		var location = GeoClueFix.Parse(FullReply)!.ToLocation();

		Assert.Equal(51.5074, location.Latitude);
		Assert.Equal(-0.1278, location.Longitude);
		Assert.Equal(25.0, location.Accuracy);
		Assert.Equal(35.5, location.Altitude);
		Assert.Equal(AltitudeReferenceSystem.Geoid, location.AltitudeReferenceSystem);
		Assert.Equal(3.5, location.Speed);
		Assert.Equal(90.0, location.Course);
	}

	[Theory]
	[InlineData(GeolocationAccuracy.Lowest, GeoClueSource.LevelCity)]
	[InlineData(GeolocationAccuracy.Low, GeoClueSource.LevelNeighborhood)]
	[InlineData(GeolocationAccuracy.Default, GeoClueSource.LevelStreet)]
	[InlineData(GeolocationAccuracy.Medium, GeoClueSource.LevelStreet)]
	[InlineData(GeolocationAccuracy.High, GeoClueSource.LevelExact)]
	[InlineData(GeolocationAccuracy.Best, GeoClueSource.LevelExact)]
	public void AccuracyMapsToGeoClueLevels(GeolocationAccuracy accuracy, uint level)
	{
		Assert.Equal(level, GeoClueSource.ToAccuracyLevel(accuracy));
	}

	[Theory]
	[InlineData(0, 0u)]
	[InlineData(-5, 0u)]
	[InlineData(1, 1u)]
	[InlineData(1500, 2u)]
	[InlineData(10000, 10u)]
	public void MinimumTimeMapsToWholeSecondThresholds(int milliseconds, uint seconds)
	{
		Assert.Equal(seconds, GeoClueSource.ToThresholdSeconds(TimeSpan.FromMilliseconds(milliseconds)));
	}

	[Theory]
	[InlineData("(objectpath '/org/freedesktop/GeoClue2/Client/7',)", "/org/freedesktop/GeoClue2/Client/7")]
	[InlineData("(objectpath '/org/freedesktop/GeoClue2/Client/7/Location/0', objectpath '/org/freedesktop/GeoClue2/Client/7/Location/1')", "/org/freedesktop/GeoClue2/Client/7/Location/1")]
	[InlineData("(objectpath '/', objectpath '/')", "/")]
	[InlineData("()", null)]
	[InlineData(null, null)]
	public void ObjectPathsAreReadFromPrintedReplies(string? printed, string? expected)
	{
		Assert.Equal(expected, GioGeoClueService.ParseObjectPath(printed));
	}

	[Theory]
	[InlineData("GDBus.Error:org.freedesktop.DBus.Error.AccessDenied: 'x' disallowed", true)]
	[InlineData("Application is not authorized to access location", true)]
	[InlineData("GDBus.Error:org.freedesktop.DBus.Error.ServiceUnknown: not provided", false)]
	public void DBusErrorsMapToUnauthorizedOnlyWhenAccessIsDenied(string message, bool unauthorized)
	{
		Assert.Equal(unauthorized, GioGeoClueService.Wrap(new Exception(message)).Unauthorized);
	}

	sealed class FakeClient : IGeoClueClient
	{
		public event EventHandler<GeoClueFix>? LocationUpdated;

		public string? DesktopId { get; private set; }

		public uint Level { get; private set; }

		public uint Threshold { get; private set; }

		public Exception? StartFailure { get; set; }

		public int StopCount;

		public int DisposeCount;

		public void Start(string desktopId, uint accuracyLevel, uint timeThresholdSeconds)
		{
			if (StartFailure is not null)
				throw StartFailure;

			DesktopId = desktopId;
			Level = accuracyLevel;
			Threshold = timeThresholdSeconds;
		}

		public void Stop() => Interlocked.Increment(ref StopCount);

		public void Dispose() => Interlocked.Increment(ref DisposeCount);

		public void Raise(GeoClueFix fix) => LocationUpdated?.Invoke(this, fix);

		public bool HasSubscribers => LocationUpdated is not null;
	}

	sealed class FakeService : IGeoClueService
	{
		public bool IsAvailable { get; set; } = true;

		public FakeClient Client { get; } = new();

		public Exception? CreateFailure { get; set; }

		public int Created;

		public IGeoClueClient CreateClient()
		{
			Interlocked.Increment(ref Created);
			if (CreateFailure is not null)
				throw CreateFailure;

			return Client;
		}
	}

	static async Task<T> Next<T>(Action<EventHandler<T>> subscribe, Action trigger)
	{
		var tcs = new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);
		subscribe((_, value) => tcs.TrySetResult(value));
		trigger();
		return await tcs.Task.WaitAsync(TimeSpan.FromSeconds(10));
	}

	static async Task WaitFor(Func<bool> condition)
	{
		for (var i = 0; i < 400 && !condition(); i++)
			await Task.Delay(25);

		Assert.True(condition(), "condition not reached");
	}

	[Fact]
	public async Task SessionConfiguresStartsAndDeliversLocations()
	{
		var service = new FakeService();
		var source = new GeoClueSource(service, "org.contoso.app");
		using var session = source.Open(GeolocationAccuracy.High, TimeSpan.FromMilliseconds(2500));

		var location = await Next<Location>(h => session.LocationChanged += h, () =>
		{
			session.Start();
			WaitFor(() => service.Client.DesktopId is not null && service.Client.HasSubscribers).GetAwaiter().GetResult();
			service.Client.Raise(GeoClueFix.Parse("({'Latitude': <10.0>, 'Longitude': <20.0>},)")!);
		});

		Assert.Equal("org.contoso.app", service.Client.DesktopId);
		Assert.Equal(GeoClueSource.LevelExact, service.Client.Level);
		Assert.Equal(3u, service.Client.Threshold);
		Assert.Equal(10, location.Latitude);
		Assert.Equal(20, location.Longitude);
	}

	[Fact]
	public async Task StartingTwiceCreatesOneClient()
	{
		var service = new FakeService();
		using var session = new GeoClueSource(service, "x").Open(GeolocationAccuracy.Default, TimeSpan.Zero);

		session.Start();
		session.Start();
		await WaitFor(() => service.Client.DesktopId is not null);

		Assert.Equal(1, service.Created);
	}

	[Fact]
	public async Task DisposeStopsAndReleasesTheClientAndSilencesLocations()
	{
		var service = new FakeService();
		var session = new GeoClueSource(service, "x").Open(GeolocationAccuracy.Default, TimeSpan.Zero);
		var count = 0;
		session.LocationChanged += (_, _) => Interlocked.Increment(ref count);
		session.Start();
		await WaitFor(() => service.Client.DesktopId is not null);

		session.Dispose();
		await WaitFor(() => service.Client.DisposeCount == 1);

		Assert.Equal(1, service.Client.StopCount);
		Assert.False(service.Client.HasSubscribers);
		service.Client.Raise(GeoClueFix.Parse("({'Latitude': <1.0>, 'Longitude': <2.0>},)")!);
		Assert.Equal(0, count);

		session.Dispose(); // idempotent
		Assert.Equal(1, service.Client.DisposeCount);
	}

	[Fact]
	public async Task DisposeBeforeTheClientExistsCreatesNothingThatLeaks()
	{
		var service = new FakeService();
		var session = new GeoClueSource(service, "x").Open(GeolocationAccuracy.Default, TimeSpan.Zero);

		session.Dispose();
		session.Start();
		await Task.Delay(100);

		Assert.Equal(0, service.Created);
	}

	[Theory]
	[InlineData(true, GeolocationError.Unauthorized)]
	[InlineData(false, GeolocationError.PositionUnavailable)]
	public async Task StartFailuresAreReportedThroughFailed(bool unauthorized, GeolocationError expected)
	{
		var service = new FakeService { Client = { StartFailure = new GeoClueException("x", unauthorized) } };
		using var session = new GeoClueSource(service, "x").Open(GeolocationAccuracy.Default, TimeSpan.Zero);

		var error = await Next<GeolocationError>(h => session.Failed += h, session.Start);

		Assert.Equal(expected, error);
	}

	[Fact]
	public async Task ClientCreationFailureIsReportedAsUnavailable()
	{
		var service = new FakeService { CreateFailure = new GeoClueException("no service", false) };
		using var session = new GeoClueSource(service, "x").Open(GeolocationAccuracy.Default, TimeSpan.Zero);

		var error = await Next<GeolocationError>(h => session.Failed += h, session.Start);

		Assert.Equal(GeolocationError.PositionUnavailable, error);
	}

	[Fact]
	public void SourceFollowsServiceAvailabilityAndHasNoCachedLocation()
	{
		var service = new FakeService { IsAvailable = false };
		var source = new GeoClueSource(service, "x");

		Assert.False(source.IsEnabled);
		service.IsAvailable = true;
		Assert.True(source.IsEnabled);
		Assert.Null(source.GetLastKnownLocation());
	}

	[Fact]
	public async Task GeolocationOverGeoClueReturnsTheFirstFix()
	{
		var service = new FakeService();
		var geo = new GtkGeolocation("org.contoso.app", new GeoClueSource(service, "org.contoso.app"));

		var task = geo.GetLocationAsync(new GeolocationRequest(GeolocationAccuracy.Best, TimeSpan.FromSeconds(30)), default);
		await WaitFor(() => service.Client.DesktopId is not null && service.Client.HasSubscribers);
		service.Client.Raise(GeoClueFix.Parse(FullReply)!);

		var location = await task.WaitAsync(TimeSpan.FromSeconds(10));
		Assert.Equal(51.5074, location!.Latitude);
		await WaitFor(() => service.Client.DisposeCount == 1);
		Assert.Equal(1, service.Client.StopCount);
	}

	[Fact]
	public void TheRealServiceIsUnavailableOrCreatesNoClientWithoutGeoClue()
	{
		var service = new GioGeoClueService();
		if (service.IsAvailable)
			return; // a machine with GeoClue: nothing to assert without side effects

		Assert.Throws<GeoClueException>(() => service.CreateClient());
	}
}

public class WindowsLocationTests
{
	sealed class FakeReports : ILocationReports
	{
		public LocationReportStatus Status { get; set; } = LocationReportStatus.Running;

		public Location? Report { get; set; }

		public int ListenInterval { get; private set; } = -1;

		public int StopCount;

		public int DisposeCount;

		public void Listen(int intervalMs) => ListenInterval = intervalMs;

		public void StopListening() => Interlocked.Increment(ref StopCount);

		public Location? ReadReport() => Report;

		public void Dispose() => Interlocked.Increment(ref DisposeCount);
	}

	static readonly TimeSpan Fast = TimeSpan.FromMilliseconds(20);

	[Theory]
	[InlineData((int)LocationReportStatus.Running, true)]
	[InlineData((int)LocationReportStatus.Initializing, true)]
	[InlineData((int)LocationReportStatus.AccessDenied, false)]
	[InlineData((int)LocationReportStatus.NotSupported, false)]
	[InlineData((int)LocationReportStatus.Error, false)]
	public void IsEnabledFollowsTheReportStatus(int status, bool enabled)
	{
		var reports = new FakeReports { Status = (LocationReportStatus)status };
		var source = new PollingLocationSource(() => reports, Fast);

		Assert.Equal(enabled, source.IsEnabled);
		Assert.Equal(1, reports.StopCount);
		Assert.Equal(1, reports.DisposeCount);
	}

	[Fact]
	public void LastKnownIsTheCurrentReport()
	{
		var reports = new FakeReports { Report = new Location(1, 2) };
		var source = new PollingLocationSource(() => reports, Fast);

		Assert.Equal(1, source.GetLastKnownLocation()!.Latitude);
		reports.Report = null;
		Assert.Null(source.GetLastKnownLocation());
	}

	[Fact]
	public async Task SessionRaisesOnlyNewReports()
	{
		var reports = new FakeReports { Report = new Location(1, 2, DateTimeOffset.UtcNow.AddMinutes(-1)) };
		var source = new PollingLocationSource(() => reports, Fast);
		using var session = source.Open(GeolocationAccuracy.Best, TimeSpan.Zero);
		var received = new List<Location>();
		session.LocationChanged += (_, l) => { lock (received) received.Add(l); };

		session.Start();
		await WaitFor(() => Count(received) == 1);
		await Task.Delay(150);
		Assert.Equal(1, Count(received)); // the same report is not repeated

		reports.Report = new Location(3, 4, DateTimeOffset.UtcNow);
		await WaitFor(() => Count(received) == 2);

		Assert.Equal(new[] { 1d, 3d }, received.Select(l => l.Latitude).ToArray());
		Assert.True(reports.ListenInterval > 0);
	}

	static int Count(List<Location> list)
	{
		lock (list)
			return list.Count;
	}

	[Fact]
	public async Task SessionWaitsWhileInitializing()
	{
		var reports = new FakeReports { Status = LocationReportStatus.Initializing, Report = new Location(1, 2) };
		using var session = new PollingLocationSource(() => reports, Fast).Open(GeolocationAccuracy.Default, TimeSpan.Zero);
		var count = 0;
		session.LocationChanged += (_, _) => Interlocked.Increment(ref count);

		session.Start();
		await Task.Delay(150);
		Assert.Equal(0, count);

		reports.Status = LocationReportStatus.Running;
		await WaitFor(() => Volatile.Read(ref count) == 1);
	}

	[Theory]
	[InlineData((int)LocationReportStatus.AccessDenied, GeolocationError.Unauthorized)]
	[InlineData((int)LocationReportStatus.NotSupported, GeolocationError.PositionUnavailable)]
	[InlineData((int)LocationReportStatus.Error, GeolocationError.PositionUnavailable)]
	public async Task SessionFailsWhenTheServiceIsDeniedOrBroken(int status, GeolocationError expected)
	{
		var reports = new FakeReports { Status = (LocationReportStatus)status };
		using var session = new PollingLocationSource(() => reports, Fast).Open(GeolocationAccuracy.Default, TimeSpan.Zero);
		var failed = new TaskCompletionSource<GeolocationError>();
		session.Failed += (_, e) => failed.TrySetResult(e);

		session.Start();

		Assert.Equal(expected, await failed.Task.WaitAsync(TimeSpan.FromSeconds(10)));
		Assert.Equal(1, reports.DisposeCount);
	}

	[Fact]
	public async Task MinimumTimeSetsThePollIntervalAndDisposeStopsEverything()
	{
		var reports = new FakeReports { Report = new Location(1, 2) };
		var source = new PollingLocationSource(() => reports, Fast);
		var session = source.Open(GeolocationAccuracy.Default, TimeSpan.FromSeconds(5));
		session.Start();
		await WaitFor(() => reports.ListenInterval >= 0);
		Assert.Equal(5000, reports.ListenInterval);

		session.Dispose();
		session.Dispose();

		Assert.Equal(1, reports.StopCount);
		Assert.Equal(1, reports.DisposeCount);
	}

	[Fact]
	public void ComReportValuesMapToALocation()
	{
		var stamp = new DateTime(2024, 5, 6, 7, 8, 9, DateTimeKind.Unspecified);
		var location = ComLocationReports.Create(10, 20, 30, 40, 5, stamp)!;

		Assert.Equal(10, location.Latitude);
		Assert.Equal(20, location.Longitude);
		Assert.Equal(30, location.Accuracy);
		Assert.Equal(40, location.Altitude);
		Assert.Equal(5, location.VerticalAccuracy);
		Assert.Equal(AltitudeReferenceSystem.Geoid, location.AltitudeReferenceSystem);
		Assert.Equal(new DateTimeOffset(2024, 5, 6, 7, 8, 9, TimeSpan.Zero), location.Timestamp);
	}

	[Fact]
	public void ComReportUnknownValuesAreNull()
	{
		var location = ComLocationReports.Create(10, 20, double.NaN, double.NaN, double.NaN, null)!;

		Assert.Null(location.Accuracy);
		Assert.Null(location.Altitude);
		Assert.Null(location.VerticalAccuracy);
		Assert.Null(ComLocationReports.Create(91, 0, null, null, null, null));
		Assert.Null(ComLocationReports.Create(0, 181, null, null, null, null));
	}

	[Fact]
	public void TheRealApiReportsAStatusWithoutThrowing()
	{
		if (!OperatingSystem.IsWindows())
			return;

		using var reports = new ComLocationReports();
		Assert.True(Enum.IsDefined(reports.Status));
		var source = new PollingLocationSource(() => new ComLocationReports());
		_ = source.IsEnabled;
		_ = source.GetLastKnownLocation();
	}

	static async Task WaitFor(Func<bool> condition)
	{
		for (var i = 0; i < 400 && !condition(); i++)
			await Task.Delay(25);

		Assert.True(condition(), "condition not reached");
	}
}

/// <summary>
/// Drives <see cref="GioGeoClueService"/> against a GeoClue2 look-alike (python3 + PyGObject) on the session bus, so the
/// real D-Bus plumbing (property writes, signal subscription, printed-variant parsing) is exercised. Linux only; skipped
/// when python3-gi is missing.
/// </summary>
[Collection("Gtk")]
public class GeoClueDBusTests
{
	const string FakeGeoClue = """
		import sys
		from gi.repository import Gio, GLib

		log = open(sys.argv[1], 'a', buffering=1)
		manager = Gio.DBusNodeInfo.new_for_xml('''<node><interface name='org.freedesktop.GeoClue2.Manager'>
		  <method name='GetClient'><arg type='o' direction='out'/></method>
		  <property name='AvailableAccuracyLevel' type='u' access='read'/></interface></node>''').interfaces[0]
		client = Gio.DBusNodeInfo.new_for_xml('''<node><interface name='org.freedesktop.GeoClue2.Client'>
		  <method name='Start'/><method name='Stop'/>
		  <property name='DesktopId' type='s' access='readwrite'/>
		  <property name='RequestedAccuracyLevel' type='u' access='readwrite'/>
		  <property name='TimeThreshold' type='u' access='readwrite'/>
		  <property name='Location' type='o' access='read'/>
		  <signal name='LocationUpdated'><arg type='o'/><arg type='o'/></signal></interface></node>''').interfaces[0]
		location = Gio.DBusNodeInfo.new_for_xml('''<node><interface name='org.freedesktop.GeoClue2.Location'>
		  <property name='Latitude' type='d' access='read'/><property name='Longitude' type='d' access='read'/>
		  <property name='Accuracy' type='d' access='read'/><property name='Altitude' type='d' access='read'/>
		  <property name='Speed' type='d' access='read'/><property name='Heading' type='d' access='read'/>
		  <property name='Description' type='s' access='read'/><property name='Timestamp' type='(tt)' access='read'/>
		  </interface></node>''').interfaces[0]

		props = {'DesktopId': GLib.Variant('s', ''), 'RequestedAccuracyLevel': GLib.Variant('u', 0), 'TimeThreshold': GLib.Variant('u', 0)}
		loc = {'Latitude': GLib.Variant('d', 48.2082), 'Longitude': GLib.Variant('d', 16.3738), 'Accuracy': GLib.Variant('d', 12.5),
		       'Altitude': GLib.Variant('d', 171.0), 'Speed': GLib.Variant('d', -1.0), 'Heading': GLib.Variant('d', 45.0),
		       'Description': GLib.Variant('s', 'Vienna'), 'Timestamp': GLib.Variant('(tt)', (1700000000, 250000))}

		def manager_call(conn, sender, path, iface, method, params, inv):
		    log.write('getclient\n')
		    inv.return_value(GLib.Variant('(o)', ('/org/freedesktop/GeoClue2/Client/1',)))

		def client_call(conn, sender, path, iface, method, params, inv):
		    if method == 'Start':
		        log.write('start desktop=%s level=%d threshold=%d\n' % (props['DesktopId'].get_string(), props['RequestedAccuracyLevel'].get_uint32(), props['TimeThreshold'].get_uint32()))
		        inv.return_value(None)
		        def emit():
		            conn.emit_signal(None, '/org/freedesktop/GeoClue2/Client/1', 'org.freedesktop.GeoClue2.Client', 'LocationUpdated',
		                             GLib.Variant('(oo)', ('/', '/org/freedesktop/GeoClue2/Location/1')))
		            return False
		        GLib.timeout_add(100, emit)
		    else:
		        log.write('stop\n')
		        inv.return_value(None)

		def client_set(conn, sender, path, iface, name, value):
		    props[name] = value
		    return True

		def acquired(conn, name):
		    conn.register_object('/org/freedesktop/GeoClue2/Manager', manager, manager_call, lambda *a: GLib.Variant('u', 8), None)
		    conn.register_object('/org/freedesktop/GeoClue2/Client/1', client, client_call,
		                         lambda c, s, p, i, n: props.get(n, GLib.Variant('o', '/')), client_set)
		    conn.register_object('/org/freedesktop/GeoClue2/Location/1', location, None, lambda c, s, p, i, n: loc[n], None)

		Gio.bus_own_name(Gio.BusType.SESSION, 'org.freedesktop.GeoClue2', Gio.BusNameOwnerFlags.NONE, acquired,
		                 lambda c, n: (print('ready', flush=True)), None)
		GLib.MainLoop().run()
		""";

	static Process? StartFakeService(string logPath)
	{
		var script = Path.Combine(Path.GetTempPath(), "maui-gtk-fake-geoclue-" + Guid.NewGuid().ToString("N") + ".py");
		File.WriteAllText(script, FakeGeoClue.Replace("\r\n", "\n"));

		try
		{
			var info = new ProcessStartInfo("python3") { RedirectStandardOutput = true, RedirectStandardError = true, UseShellExecute = false };
			info.ArgumentList.Add(script);
			info.ArgumentList.Add(logPath);
			var process = Process.Start(info)!;
			var ready = process.StandardOutput.ReadLineAsync();
			if (!ready.Wait(TimeSpan.FromSeconds(15)) || ready.Result != "ready")
			{
				try { process.Kill(); } catch (InvalidOperationException) { }
				return null;
			}

			return process;
		}
		catch (System.ComponentModel.Win32Exception)
		{
			return null; // no python3
		}
		finally
		{
			try { File.Delete(script); } catch (IOException) { }
		}
	}

	[GtkFact]
	public async Task RealDBusPlumbingConfiguresTheClientAndParsesTheLocation()
	{
		if (!OperatingSystem.IsLinux() || string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DBUS_SESSION_BUS_ADDRESS")))
			return;

		var host = GtkHost.Instance!;
		var logPath = Path.Combine(Path.GetTempPath(), "maui-gtk-geoclue-" + Guid.NewGuid().ToString("N") + ".log");
		using var process = StartFakeService(logPath);
		if (process is null)
			return; // python3 / PyGObject unavailable

		try
		{
			var service = new GioGeoClueService(GLib.BusType.Session);
			Assert.True(service.IsAvailable);

			var geo = new GtkGeolocation("org.maui.gtk.tests", new GeoClueSource(service, "org.maui.gtk.tests"));
			var task = geo.GetLocationAsync(new GeolocationRequest(GeolocationAccuracy.Best, TimeSpan.FromSeconds(20)), default);
			host.Invoke(() => GtkTestHost.PumpUntil(() => task.IsCompleted, timeoutMs: 20000));

			var location = await task.WaitAsync(TimeSpan.FromSeconds(5));
			Assert.NotNull(location);
			Assert.Equal(48.2082, location!.Latitude);
			Assert.Equal(16.3738, location.Longitude);
			Assert.Equal(12.5, location.Accuracy);
			Assert.Equal(171.0, location.Altitude);
			Assert.Null(location.Speed);
			Assert.Equal(45.0, location.Course);
			Assert.Equal(DateTimeOffset.FromUnixTimeSeconds(1700000000).AddTicks(2_500_000), location.Timestamp);

			var deadline = DateTime.UtcNow.AddSeconds(10);
			string log;
			while (!(log = File.Exists(logPath) ? File.ReadAllText(logPath) : "").Contains("stop") && DateTime.UtcNow < deadline)
				await Task.Delay(50);

			Assert.Contains("start desktop=org.maui.gtk.tests level=8 threshold=0", log);
			Assert.Contains("stop", log);
		}
		finally
		{
			try { process.Kill(); } catch (InvalidOperationException) { }
			try { File.Delete(logPath); } catch (IOException) { }
		}
	}
}
