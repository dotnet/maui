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
            await Permissions.RequireAsync(PermissionType.LocationWhenInUse);

            var geolocator = new Geolocator
            {
                DesiredAccuracyInMeters = request.PlatformDesiredAccuracy
            };

            cancellationToken = Utils.TimeoutToken(cancellationToken, request.Timeout);

            var location = await geolocator.GetGeopositionAsync().AsTask(cancellationToken);

            return location?.Coordinate?.ToLocation();
        }
    }
}
