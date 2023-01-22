using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CoreLocation;
using Microsoft.Maui.ApplicationModel;

namespace Microsoft.Maui.Devices.Sensors
{
	partial class GeolocationImplementation : IGeolocation
	{
		CLLocationManager listeningManager;

		public bool IsListening { get => listeningManager != null; }

		public async Task<Location> GetLastKnownLocationAsync()
		{
			if (!CLLocationManager.LocationServicesEnabled)
				throw new FeatureNotEnabledException("Location services are not enabled on device.");

			await Permissions.EnsureGrantedAsync<Permissions.LocationWhenInUse>();

			var manager = new CLLocationManager();
			var location = manager.Location;

			var reducedAccuracy = false;
#if __IOS__
			if (OperatingSystem.IsIOSVersionAtLeast(14, 0))
			{
				reducedAccuracy = manager.AccuracyAuthorization == CLAccuracyAuthorization.ReducedAccuracy;
			}
#endif
			return location?.ToLocation(reducedAccuracy);
		}

		public async Task<Location> GetLocationAsync(GeolocationRequest request, CancellationToken cancellationToken)
		{
			_ = request ?? throw new ArgumentNullException(nameof(request));

			if (!CLLocationManager.LocationServicesEnabled)
				throw new FeatureNotEnabledException("Location services are not enabled on device.");

			await Permissions.EnsureGrantedAsync<Permissions.LocationWhenInUse>();

			// the location manager requires an active run loop
			// so just use the main loop
			var manager = MainThread.InvokeOnMainThread(() => new CLLocationManager());

			var tcs = new TaskCompletionSource<CLLocation>(manager);

			var listener = new SingleLocationListener();
			listener.LocationHandler += HandleLocation;

			cancellationToken = Utils.TimeoutToken(cancellationToken, request.Timeout);
			cancellationToken.Register(Cancel);

			manager.DesiredAccuracy = request.PlatformDesiredAccuracy;
			manager.Delegate = listener;

#if __IOS__
			// we're only listening for a single update
#pragma warning disable CA1416 // https://github.com/xamarin/xamarin-macios/issues/14619
			manager.PausesLocationUpdatesAutomatically = false;
#pragma warning restore CA1416
#endif

			manager.StartUpdatingLocation();

			var reducedAccuracy = false;
#if __IOS__
			if (OperatingSystem.IsIOSVersionAtLeast(14, 0))
			{
				if (request.RequestFullAccuracy && manager.AccuracyAuthorization == CLAccuracyAuthorization.ReducedAccuracy)
				{
					await manager.RequestTemporaryFullAccuracyAuthorizationAsync("TemporaryFullAccuracyUsageDescription");
				}

				reducedAccuracy = manager.AccuracyAuthorization == CLAccuracyAuthorization.ReducedAccuracy;
			}
#endif

			var clLocation = await tcs.Task;

			return clLocation?.ToLocation(reducedAccuracy);

			void HandleLocation(CLLocation location)
			{
				manager.StopUpdatingLocation();
				tcs.TrySetResult(location);
			}

			void Cancel()
			{
				manager.StopUpdatingLocation();
				tcs.TrySetResult(null);
			}
		}

		public async Task<bool> StartListeningForegroundAsync(GeolocationListeningRequest request)
		{
			_ = request ?? throw new ArgumentNullException(nameof(request));

			if (IsListening)
				throw new InvalidOperationException("Already listening to location changes.");

			if (!CLLocationManager.LocationServicesEnabled)
				throw new FeatureNotEnabledException("Location services are not enabled on device.");

			await Permissions.EnsureGrantedAsync<Permissions.LocationWhenInUse>();

			// the location manager requires an active run loop
			// so just use the main loop
			listeningManager = MainThread.InvokeOnMainThread(() => new CLLocationManager());

			var reducedAccuracy = false;
#if __IOS__
			if (OperatingSystem.IsIOSVersionAtLeast(14, 0))
			{
				reducedAccuracy = listeningManager.AccuracyAuthorization == CLAccuracyAuthorization.ReducedAccuracy;
			}
#endif

			var listener = new ContinuousLocationListener();
			listener.LocationHandler += HandleLocation;

			listeningManager.DesiredAccuracy = request.PlatformDesiredAccuracy;
			listeningManager.Delegate = listener;

#if __IOS__
#pragma warning disable CA1416 // https://github.com/xamarin/xamarin-macios/issues/14619
			// allow pausing updates
			listeningManager.PausesLocationUpdatesAutomatically = true;
#pragma warning restore CA1416
#endif

			listeningManager.StartUpdatingLocation();

			return true;

			void HandleLocation(CLLocation clLocation)
			{
				OnLocationChanged(clLocation?.ToLocation(reducedAccuracy));
			}
		}

		public Task<bool> StopListeningForegroundAsync()
		{
			if (!IsListening)
				return Task.FromResult(true);

			listeningManager.StopUpdatingLocation();
			listeningManager.Delegate = null;

			listeningManager = null;

			return Task.FromResult(true);
		}
	}

	class SingleLocationListener : CLLocationManagerDelegate
	{
		bool wasRaised = false;

		internal Action<CLLocation> LocationHandler { get; set; }

		public override void LocationsUpdated(CLLocationManager manager, CLLocation[] locations)
		{
			if (wasRaised)
				return;

			wasRaised = true;

			var location = locations?.LastOrDefault();

			if (location == null)
				return;

			LocationHandler?.Invoke(location);
		}

		public override bool ShouldDisplayHeadingCalibration(CLLocationManager manager) => false;
	}

	class ContinuousLocationListener : CLLocationManagerDelegate
	{
		internal Action<CLLocation> LocationHandler { get; set; }

		public override void LocationsUpdated(CLLocationManager manager, CLLocation[] locations)
		{
			var location = locations?.LastOrDefault();

			if (location == null)
				return;

			LocationHandler?.Invoke(location);
		}

		public override bool ShouldDisplayHeadingCalibration(CLLocationManager manager) => false;
	}
}
