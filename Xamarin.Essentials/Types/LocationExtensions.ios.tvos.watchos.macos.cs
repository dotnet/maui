using System;
using System.Collections.Generic;
using System.Linq;
using CoreLocation;
using Foundation;

namespace Xamarin.Essentials
{
    public static partial class LocationExtensions
    {
        internal static Location ToLocation(this CLPlacemark placemark) =>
            new Location
            {
                Latitude = placemark.Location.Coordinate.Latitude,
                Longitude = placemark.Location.Coordinate.Longitude,
                Altitude = placemark.Location.Altitude,
                AltitudeReferenceSystem = AltitudeReferenceSystem.Geoid,
                Timestamp = DateTimeOffset.UtcNow
            };

        internal static IEnumerable<Location> ToLocations(this IEnumerable<CLPlacemark> placemarks) =>
            placemarks?.Select(a => a.ToLocation());

        internal static Location ToLocation(this CLLocation location) =>
            new Location
            {
                Latitude = location.Coordinate.Latitude,
                Longitude = location.Coordinate.Longitude,
                Altitude = location.VerticalAccuracy < 0 ? default(double?) : location.Altitude,
                Accuracy = location.HorizontalAccuracy,
                VerticalAccuracy = location.VerticalAccuracy,
                Timestamp = location.Timestamp.ToDateTime(),
#if __iOS__ || __WATCHOS__
                Course = location.Course < 0 ? default(double?) : location.Course,
                Speed = location.Speed < 0 ? default(double?) : location.Speed,
#endif
                IsFromMockProvider = DeviceInfo.DeviceType == DeviceType.Virtual,
                AltitudeReferenceSystem = AltitudeReferenceSystem.Geoid
            };

        internal static DateTimeOffset ToDateTime(this NSDate timestamp)
        {
            try
            {
                return new DateTimeOffset((DateTime)timestamp);
            }
            catch
            {
                return DateTimeOffset.UtcNow;
            }
        }

        internal static CLAuthorizationStatus GetAuthorizationStatus(this CLLocationManager locationManager)
        {
#if !__MACOS__ // this is coming in macOS 11
#if __WATCHOS__
            if (Platform.HasOSVersion(7, 0))
#else
            if (Platform.HasOSVersion(14, 0))
#endif
                return locationManager.AuthorizationStatus;

#endif

            return CLLocationManager.Status;
        }
    }
}
