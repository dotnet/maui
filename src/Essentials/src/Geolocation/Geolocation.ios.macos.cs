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
		public async Task<Location> GetLastKnownLocationAsync()
		{
			if (!CLLocationManager.LocationServicesEnabled)
				throw new FeatureNotEnabledException("Location services are not enabled on device.");

			await Permissions.EnsureGrantedAsync<Permissions.LocationWhenInUse>();

			var manager = new CLLocationManager();
			var location = manager.Location;

			return location?.ToLocation();
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
			manager.PausesLocationUpdatesAutomatically = false;
#endif

			manager.StartUpdatingLocation();

			var clLocation = await tcs.Task;

			return clLocation?.ToLocation();

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
}
