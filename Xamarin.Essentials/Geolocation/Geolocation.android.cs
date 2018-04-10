using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Android.Locations;
using Android.OS;
using Android.Runtime;

using AndroidLocation = Android.Locations.Location;

namespace Xamarin.Essentials
{
    public static partial class Geolocation
    {
        const long twoMinutes = 120000;

        static async Task<Location> PlatformLastKnownLocationAsync()
        {
            await Permissions.RequireAsync(PermissionType.LocationWhenInUse);

            var lm = Platform.LocationManager;
            AndroidLocation bestLocation = null;

            foreach (var provider in lm.GetProviders(true))
            {
                var location = lm.GetLastKnownLocation(provider);

                if (bestLocation == null || IsBetterLocation(location, bestLocation))
                    bestLocation = location;
            }

            if (bestLocation == null)
                return null;

            return bestLocation.ToLocation();
        }

        static async Task<Location> PlatformLocationAsync(GeolocationRequest request, CancellationToken cancellationToken)
        {
            await Permissions.RequireAsync(PermissionType.LocationWhenInUse);

            var locationManager = Platform.LocationManager;

            // get the best possible provider for the requested accuracy
            var provider = GetBestProvider(locationManager, request.DesiredAccuracy);

            // if no providers exist, we can't get a location
            // let's punt and try to get the last known location
            if (string.IsNullOrEmpty(provider))
                return await GetLastKnownLocationAsync();

            var tcs = new TaskCompletionSource<AndroidLocation>();

            var listener = new SingleLocationListener();
            listener.LocationHandler = HandleLocation;

            cancellationToken = Utils.TimeoutToken(cancellationToken, request.Timeout);
            cancellationToken.Register(Cancel);

            // start getting location updates
            // make sure to use a thread with a looper
            var looper = Looper.MyLooper() ?? Looper.MainLooper;
            locationManager.RequestLocationUpdates(provider, 0, 0, listener, looper);

            var androidLocation = await tcs.Task;

            if (androidLocation == null)
                return null;

            return androidLocation.ToLocation();

            void HandleLocation(AndroidLocation location)
            {
                RemoveUpdates();
                tcs.TrySetResult(location);
            }

            void Cancel()
            {
                RemoveUpdates();
                tcs.TrySetResult(null);
            }

            void RemoveUpdates()
            {
                locationManager.RemoveUpdates(listener);
            }
        }

        class SingleLocationListener : Java.Lang.Object, ILocationListener
        {
            bool wasRaised = false;

            public Action<AndroidLocation> LocationHandler { get; set; }

            public void OnLocationChanged(AndroidLocation location)
            {
                if (wasRaised)
                    return;

                wasRaised = true;

                LocationHandler?.Invoke(location);
            }

            public void OnProviderDisabled(string provider)
            {
            }

            public void OnProviderEnabled(string provider)
            {
            }

            public void OnStatusChanged(string provider, [GeneratedEnum] Availability status, Bundle extras)
            {
            }
        }

        static string GetBestProvider(LocationManager locationManager, GeolocationAccuracy accuracy)
        {
            var criteria = new Criteria();
            criteria.BearingRequired = false;
            criteria.AltitudeRequired = false;
            criteria.SpeedRequired = false;

            switch (accuracy)
            {
                case GeolocationAccuracy.Lowest:
                    criteria.Accuracy = Accuracy.NoRequirement;
                    criteria.HorizontalAccuracy = Accuracy.NoRequirement;
                    criteria.PowerRequirement = Power.NoRequirement;
                    break;
                case GeolocationAccuracy.Low:
                    criteria.Accuracy = Accuracy.Low;
                    criteria.HorizontalAccuracy = Accuracy.Low;
                    criteria.PowerRequirement = Power.Low;
                    break;
                case GeolocationAccuracy.Medium:
                    criteria.Accuracy = Accuracy.Medium;
                    criteria.HorizontalAccuracy = Accuracy.Medium;
                    criteria.PowerRequirement = Power.Medium;
                    break;
                case GeolocationAccuracy.High:
                    criteria.Accuracy = Accuracy.High;
                    criteria.HorizontalAccuracy = Accuracy.High;
                    criteria.PowerRequirement = Power.High;
                    break;
                case GeolocationAccuracy.Best:
                    criteria.Accuracy = Accuracy.Fine;
                    criteria.HorizontalAccuracy = Accuracy.Fine;
                    criteria.PowerRequirement = Power.High;
                    break;
            }

            return locationManager.GetBestProvider(criteria, true) ?? locationManager.GetProviders(true).FirstOrDefault();
        }

        internal static bool IsBetterLocation(AndroidLocation location, AndroidLocation bestLocation)
        {
            if (bestLocation == null)
                return true;

            var timeDelta = location.Time - bestLocation.Time;

            var isSignificantlyNewer = timeDelta > twoMinutes;
            var isSignificantlyOlder = timeDelta < -twoMinutes;
            var isNewer = timeDelta > 0;

            if (isSignificantlyNewer)
                return true;

            if (isSignificantlyOlder)
                return false;

            var accuracyDelta = (int)(location.Accuracy - bestLocation.Accuracy);
            var isLessAccurate = accuracyDelta > 0;
            var isMoreAccurate = accuracyDelta < 0;
            var isSignificantlyLessAccurage = accuracyDelta > 200;

            var isFromSameProvider = location?.Provider?.Equals(bestLocation?.Provider, StringComparison.OrdinalIgnoreCase) ?? false;

            if (isMoreAccurate)
                return true;

            if (isNewer && !isLessAccurate)
                return true;

            if (isNewer && !isSignificantlyLessAccurage && isFromSameProvider)
                return true;

            return false;
        }
    }
}
