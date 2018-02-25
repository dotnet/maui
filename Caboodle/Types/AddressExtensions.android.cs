using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Caboodle
{
	public static partial class AddressExtensions
	{
        internal static IEnumerable<Address> ToAddresses(this IEnumerable<Android.Locations.Address> addresses)
        {
            return addresses.Select(address => new Address
            {
                Longitude = address.Longitude,
                Latitude = address.Latitude,
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
