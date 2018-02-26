using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Caboodle
{
	public static partial class PlacemarkExtensions
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

        internal static Location ToLocation(this Android.Locations.Location location)
        {
			var p = new Location
			{
				Longitude = location.Longitude,
				Latitude = location.Latitude,
				TimestampUtc = location.GetTimestamp().ToUniversalTime()
			};
			return p;
        }

		internal static IEnumerable<Placemark> ToPlacemarks(this IEnumerable<Android.Locations.Address> addresses)
		{
			return addresses.Select(address => new Placemark
			{
				Location = new Location(address.Latitude, address.Longitude),
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
