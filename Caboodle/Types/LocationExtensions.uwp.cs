using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Services.Maps;

namespace Microsoft.Caboodle
{
    public static partial class LocationExtensions
    {
        internal static Location ToLocation(this MapLocation mapLocation) =>
            new Location(mapLocation.Point.Position.Latitude, mapLocation.Point.Position.Longitude, DateTimeOffset.UtcNow);

        internal static IEnumerable<Location> ToLocations(this IEnumerable<MapLocation> mapLocations) =>
            mapLocations?.Select(a => a.ToLocation());

        internal static IEnumerable<Location> ToLocations(this MapLocationFinderResult result) =>
            result?.ToLocations();
    }
}
