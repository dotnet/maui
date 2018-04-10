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
                TimestampUtc = DateTimeOffset.UtcNow
            };

        internal static IEnumerable<Location> ToLocations(this IEnumerable<CLPlacemark> placemarks) =>
            placemarks?.Select(a => a.ToLocation());

        internal static Location ToLocation(this CLLocation location) =>
            new Location
            {
                Latitude = location.Coordinate.Latitude,
                Longitude = location.Coordinate.Longitude,
                Accuracy = location.HorizontalAccuracy,
                TimestampUtc = location.Timestamp.ToDateTime()
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
    }
}
