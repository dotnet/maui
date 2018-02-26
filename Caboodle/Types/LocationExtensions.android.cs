using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Caboodle
{
	public static partial class PositionExtensions
	{
        private static readonly DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        internal static DateTimeOffset GetTimestamp(this Android.Locations.Location location)
        {
            try
            {
                return new DateTimeOffset(epoch.AddMilliseconds(location.Time));
            }
            catch (Exception e)
            {
                return new DateTimeOffset(epoch);
            }
        }

        internal static GeoPoint ToPoint(this Android.Locations.Location location)
        {
			var p = new GeoPoint
			{
				Longitude = location.Longitude,
				Latitude = location.Latitude,
				TimestampUtc = location.GetTimestamp().ToUniversalTime()
			};
			return p;
        }

		internal static IEnumerable<Location> ToLocations(this IEnumerable<Android.Locations.Address> addresses)
		{
			return addresses.Select(address => new Location
			{
				Point = new GeoPoint(address.Latitude, address.Longitude),
				FeatureName = address.FeatureName,
				PostalCode = address.PostalCode,
				SubLocality = address.SubLocality,
				CountryCode = address.CountryCode,
				CountryName = address.CountryName,
				Thoroughfare = address.Thoroughfare,
				SubThoroughfare = address.SubThoroughfare,
				Locality = address.Locality,
				AdminArea = address.AdminArea,
				SubAdminArea = address.SubAdminArea
			});
		}
	}
}
