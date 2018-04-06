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
            new Location(address.Latitude, address.Longitude, DateTimeOffset.UtcNow);

        internal static IEnumerable<Location> ToLocations(this IEnumerable<AndroidAddress> addresses) =>
            addresses?.Select(a => a.ToLocation());

        internal static Location ToLocation(this AndroidLocation location) =>
            new Location(location.Latitude, location.Longitude, location.GetTimestamp().ToUniversalTime());

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
