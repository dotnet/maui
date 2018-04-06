using System;
using System.Collections.Generic;
using System.Linq;
using CoreLocation;

namespace Xamarin.Essentials
{
    public static partial class LocationExtensions
    {
        internal static Location ToLocation(this CLPlacemark placemark) =>
            new Location(placemark.Location.Coordinate.Latitude, placemark.Location.Coordinate.Longitude, DateTimeOffset.UtcNow);

        internal static IEnumerable<Location> ToLocations(this IEnumerable<CLPlacemark> placemarks) =>
            placemarks?.Select(a => a.ToLocation());
    }
}
