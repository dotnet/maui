using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Android.Locations;
using Android.OS;
using Android.Runtime;

using AndroidLocation = Android.Locations.Location;
using LocationPower = Android.Locations.Power;

namespace Xamarin.Essentials
{
    public static partial class Geolocation
    {
        const long twoMinutes = 120000;
        static readonly string[] ignoredProviders = new string[] { LocationManager.PassiveProvider, "local_database" };

        static async Task<Location> PlatformLastKnownLocationAsync()
        {
            await Permissions.RequestAndVerifyAsync<Permissions.LocationWhenInUse>(nameof(Permissions.LocationWhenInUse));

            var lm = Platform.LocationManager;
            AndroidLocation bestLocation = null;

            foreach (var provider in lm.GetProviders(true))
            {
                var location = lm.GetLastKnownLocation(provider);

                if (location != null && IsBetterLocation(location, bestLocation))
                    bestLocation = location;
            }

            return bestLocation?.ToLocation();
        }

        static async Task<Location> PlatformLocationAsync(GeolocationRequest request, CancellationToken cancellationToken)
        {
            await Permissions.RequestAndVerifyAsync<Permissions.LocationWhenInUse>(nameof(Permissions.LocationWhenInUse));

            var locationManager = Platform.LocationManager;

            var enabledProviders = locationManager.GetProviders(true);
            var hasProviders = enabledProviders.Any(p => !ignoredProviders.Contains(p));

            if (!hasProviders)
                throw new FeatureNotEnabledException("Location services are not enabled on device.");

            // get the best possible provider for the requested accuracy
            var providerInfo = GetBestProvider(locationManager, request.DesiredAccuracy);

            // if no providers exist, we can't get a location
            // let's punt and try to get the last known location
            if (string.IsNullOrEmpty(providerInfo.Provider))
                return await GetLastKnownLocationAsync();

            var tcs = new TaskCompletionSource<AndroidLocation>();

            var allProviders = locationManager.GetProviders(false);

            var providers = new List<string>();
            if (allProviders.Contains(LocationManager.GpsProvider))
                providers.Add(LocationManager.GpsProvider);
            if (allProviders.Contains(LocationManager.NetworkProvider))
                providers.Add(LocationManager.NetworkProvider);

            if (providers.Count == 0)
                providers.Add(providerInfo.Provider);

            var listener = new SingleLocationListener(locationManager, providerInfo.Accuracy, providers);
            listener.LocationHandler = HandleLocation;

            cancellationToken = Utils.TimeoutToken(cancellationToken, request.Timeout);
            cancellationToken.Register(Cancel);

            // start getting location updates
            // make sure to use a thread with a looper
            var looper = Looper.MyLooper() ?? Looper.MainLooper;

            foreach (var provider in providers)
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
                tcs.TrySetResult(listener.BestLocation);
            }

            void RemoveUpdates()
            {
                for (var i = 0; i < providers.Count; i++)
                    locationManager.RemoveUpdates(listener);
            }
        }

        static (string Provider, float Accuracy) GetBestProvider(LocationManager locationManager, GeolocationAccuracy accuracy)
        {
            // Criteria: https://developer.android.com/reference/android/location/Criteria

            var criteria = new Criteria
            {
                BearingRequired = false,
                AltitudeRequired = false,
                SpeedRequired = false
            };

            var accuracyDistance = 100;

            switch (accuracy)
            {
                case GeolocationAccuracy.Lowest:
                    criteria.Accuracy = Accuracy.NoRequirement;
                    criteria.HorizontalAccuracy = Accuracy.NoRequirement;
                    criteria.PowerRequirement = LocationPower.NoRequirement;
                    accuracyDistance = 500;
                    break;
                case GeolocationAccuracy.Low:
                    criteria.Accuracy = Accuracy.Coarse;
                    criteria.HorizontalAccuracy = Accuracy.Low;
                    criteria.PowerRequirement = LocationPower.Low;
                    accuracyDistance = 500;
                    break;
                case GeolocationAccuracy.Default:
                case GeolocationAccuracy.Medium:
                    criteria.Accuracy = Accuracy.Coarse;
                    criteria.HorizontalAccuracy = Accuracy.Medium;
                    criteria.PowerRequirement = LocationPower.Medium;
                    accuracyDistance = 250;
                    break;
                case GeolocationAccuracy.High:
                    criteria.Accuracy = Accuracy.Fine;
                    criteria.HorizontalAccuracy = Accuracy.High;
                    criteria.PowerRequirement = LocationPower.High;
                    accuracyDistance = 100;
                    break;
                case GeolocationAccuracy.Best:
                    criteria.Accuracy = Accuracy.Fine;
                    criteria.HorizontalAccuracy = Accuracy.High;
                    criteria.PowerRequirement = LocationPower.High;
                    accuracyDistance = 50;
                    break;
            }

            var provider = locationManager.GetBestProvider(criteria, true) ?? locationManager.GetProviders(true).FirstOrDefault();

            return (provider, accuracyDistance);
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

    class SingleLocationListener : Java.Lang.Object, ILocationListener
    {
        readonly object locationSync = new object();

        float desiredAccuracy;

        internal AndroidLocation BestLocation { get; set; }

        HashSet<string> activeProviders = new HashSet<string>();

        bool wasRaised = false;

        internal Action<AndroidLocation> LocationHandler { get; set; }

        internal SingleLocationListener(LocationManager manager, float desiredAccuracy, IEnumerable<string> activeProviders)
        {
            this.desiredAccuracy = desiredAccuracy;

            this.activeProviders = new HashSet<string>(activeProviders);

            foreach (var provider in activeProviders)
            {
                var location = manager.GetLastKnownLocation(provider);
                if (location != null && Geolocation.IsBetterLocation(location, BestLocation))
                    BestLocation = location;
            }
        }

        void ILocationListener.OnLocationChanged(AndroidLocation location)
        {
            if (location.Accuracy <= desiredAccuracy)
            {
                if (wasRaised)
                    return;

                wasRaised = true;

                LocationHandler?.Invoke(location);
                return;
            }

            lock (locationSync)
            {
                if (Geolocation.IsBetterLocation(location, BestLocation))
                    BestLocation = location;
            }
        }

        void ILocationListener.OnProviderDisabled(string provider)
        {
            lock (activeProviders)
                activeProviders.Remove(provider);
        }

        void ILocationListener.OnProviderEnabled(string provider)
        {
            lock (activeProviders)
                activeProviders.Add(provider);
        }

        void ILocationListener.OnStatusChanged(string provider, [GeneratedEnum] Availability status, Bundle extras)
        {
            switch (status)
            {
                case Availability.Available:
                    ((ILocationListener)this).OnProviderEnabled(provider);
                    break;

                case Availability.OutOfService:
                    ((ILocationListener)this).OnProviderDisabled(provider);
                    break;
            }
        }
    }
}
