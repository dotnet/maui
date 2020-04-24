using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;

namespace Xamarin.Essentials
{
    public static partial class Geolocation
    {
        static async Task<Location> PlatformLastKnownLocationAsync()
        {
            // no need for permissions as AllowFallbackToConsentlessPositions
            // will allow the device to return a location regardless

            var geolocator = new Geolocator
            {
                DesiredAccuracy = PositionAccuracy.Default,
            };
            geolocator.AllowFallbackToConsentlessPositions();

            var location = await geolocator.GetGeopositionAsync().AsTask();

            return location?.Coordinate?.ToLocation();
        }

        static async Task<Location> PlatformLocationAsync(GeolocationRequest request, CancellationToken cancellationToken)
        {
            var permission = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();

            if (permission != PermissionStatus.Granted)
                throw new PermissionException($"LocationWhenInUse was not granted: {permission}");

            var geolocator = new Geolocator
            {
                DesiredAccuracyInMeters = request.PlatformDesiredAccuracy
            };

            CheckStatus(geolocator.LocationStatus);

            cancellationToken = Utils.TimeoutToken(cancellationToken, request.Timeout);

            var location = await geolocator.GetGeopositionAsync().AsTask(cancellationToken);

            return location?.Coordinate?.ToLocation();

            void CheckStatus(PositionStatus status)
            {
                switch (status)
                {
                    case PositionStatus.Disabled:
                    case PositionStatus.NotAvailable:
                        throw new FeatureNotEnabledException("Location services are not enabled on device.");
                }
            }
        }
    }
}
