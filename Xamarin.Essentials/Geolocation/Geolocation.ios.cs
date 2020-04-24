using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CoreLocation;
using Foundation;

namespace Xamarin.Essentials
{
    public static partial class Geolocation
    {
        static async Task<Location> PlatformLastKnownLocationAsync()
        {
            if (!CLLocationManager.LocationServicesEnabled)
                throw new FeatureNotEnabledException("Location services are not enabled on device.");

            var permission = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();

            if (permission != PermissionStatus.Granted)
                throw new PermissionException($"LocationWhenInUse was not granted: {permission}");

            var manager = new CLLocationManager();
            var location = manager.Location;

            return location?.ToLocation();
        }

        static async Task<Location> PlatformLocationAsync(GeolocationRequest request, CancellationToken cancellationToken)
        {
            if (!CLLocationManager.LocationServicesEnabled)
                throw new FeatureNotEnabledException("Location services are not enabled on device.");

            var permission = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();

            if (permission != PermissionStatus.Granted)
                throw new PermissionException($"LocationWhenInUse was not granted: {permission}");

            // the location manager requires an active run loop
            // so just use the main loop
            CLLocationManager manager = null;
            NSRunLoop.Main.InvokeOnMainThread(() => manager = new CLLocationManager());

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
