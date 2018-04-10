using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Devices.Geolocation;
using Windows.Services.Maps;

namespace Xamarin.Essentials
{
    public static partial class LocationExtensions
    {
        internal static Location ToLocation(this MapLocation mapLocation) =>
            new Location
            {
                Latitude = mapLocation.Point.Position.Latitude,
                Longitude = mapLocation.Point.Position.Longitude,
                TimestampUtc = DateTimeOffset.UtcNow
            };

        internal static IEnumerable<Location> ToLocations(this IEnumerable<MapLocation> mapLocations) =>
            mapLocations?.Select(a => a.ToLocation());

        internal static IEnumerable<Location> ToLocations(this MapLocationFinderResult result) =>
            result?.ToLocations();

        internal static Location ToLocation(this Geoposition location) =>
             new Location
             {
                 Latitude = location.Coordinate.Point.Position.Latitude,
                 Longitude = location.Coordinate.Point.Position.Longitude,
                 TimestampUtc = location.Coordinate.Timestamp,
                 Accuracy = location.Coordinate.Accuracy
             };
    }
}
