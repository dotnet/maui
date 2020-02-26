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
                Altitude = mapLocation.Point.Position.Altitude,
                Timestamp = DateTimeOffset.UtcNow
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
                Timestamp = location.Coordinate.Timestamp,
                Altitude = location.Coordinate.Point.Position.Altitude,
                Accuracy = location.Coordinate.Accuracy,
                VerticalAccuracy = location.Coordinate.AltitudeAccuracy,
                Speed = (!location.Coordinate.Speed.HasValue || double.IsNaN(location.Coordinate.Speed.Value)) ? default : location.Coordinate.Speed,
                Course = (!location.Coordinate.Heading.HasValue || double.IsNaN(location.Coordinate.Heading.Value)) ? default : location.Coordinate.Heading,
                IsFromMockProvider = false
            };

        internal static Location ToLocation(this Geocoordinate coordinate) =>
             new Location
             {
                 Latitude = coordinate.Point.Position.Latitude,
                 Longitude = coordinate.Point.Position.Longitude,
                 Timestamp = coordinate.Timestamp,
                 Altitude = coordinate.Point.Position.Altitude,
                 Accuracy = coordinate.Accuracy,
                 VerticalAccuracy = coordinate.AltitudeAccuracy,
                 Speed = (!coordinate.Speed.HasValue || double.IsNaN(coordinate.Speed.Value)) ? default : coordinate.Speed,
                 Course = (!coordinate.Heading.HasValue || double.IsNaN(coordinate.Heading.Value)) ? default : coordinate.Heading
             };
    }
}
