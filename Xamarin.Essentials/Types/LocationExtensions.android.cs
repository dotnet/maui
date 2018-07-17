using System;
using System.Collections.Generic;
using System.Linq;

using AndroidAddress = Android.Locations.Address;
using AndroidLocation = Android.Locations.Location;

namespace Xamarin.Essentials
{
    public static partial class LocationExtensions
    {
        internal static Location ToLocation(this AndroidAddress address) =>
            new Location
            {
                Latitude = address.Latitude,
                Longitude = address.Longitude,
                TimestampUtc = DateTimeOffset.UtcNow
            };

        internal static IEnumerable<Location> ToLocations(this IEnumerable<AndroidAddress> addresses) =>
            addresses?.Select(a => a.ToLocation());

        internal static Location ToLocation(this AndroidLocation location) =>
            new Location
            {
                Latitude = location.Latitude,
                Longitude = location.Longitude,
                Altitude = location.HasAltitude ? location.Altitude : (double?)null,
                TimestampUtc = location.GetTimestamp().ToUniversalTime(),
                Accuracy = location.HasAccuracy ? location.Accuracy : (float?)null
            };

        static readonly DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        internal static DateTimeOffset GetTimestamp(this AndroidLocation location)
        {
            try
            {
                return new DateTimeOffset(epoch.AddMilliseconds(location.Time));
            }
            catch (Exception)
            {
                return new DateTimeOffset(epoch);
            }
        }
    }
}
