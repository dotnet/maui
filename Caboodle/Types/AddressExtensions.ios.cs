using CoreLocation;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Caboodle
{
	public static partial class AddressExtensions
    {
		internal static IEnumerable<Address> ToAddresses(this IEnumerable<CLPlacemark> addresses)
		{
			return addresses.Select(address => new Address
			{
				Longitude = address.Location.Coordinate.Longitude,
				Latitude = address.Location.Coordinate.Latitude,
				FeatureName = address.Name,
				PostalCode = address.PostalCode,
				SubLocality = address.SubLocality,
				CountryCode = address.IsoCountryCode,
				CountryName = address.Country,
				Thoroughfare = address.Thoroughfare,
				SubThoroughfare = address.SubThoroughfare,
				Locality = address.Locality,
				AdminArea = address.AdministrativeArea,
				SubAdminArea = address.SubAdministrativeArea
			});
		}
	}
}
