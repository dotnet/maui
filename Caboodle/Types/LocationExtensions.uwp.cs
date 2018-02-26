using System.Collections.Generic;
using System.Linq;
using Windows.Services.Maps;

namespace Microsoft.Caboodle
{
	public static partial class LoctionExtensions
    {
		internal static IEnumerable<Location> ToAddresses(this IEnumerable<MapLocation> addresses)
		{
			return addresses.Select(address => new Location
			{
				Point = new GeoPoint(address.Point.Position.Latitude, address.Point.Position.Longitude),
				FeatureName = address.DisplayName,
				PostalCode = address.Address.PostCode,
				CountryCode = address.Address.CountryCode,
				CountryName = address.Address.Country,
				Thoroughfare = address.Address.Street,
				SubThoroughfare = address.Address.StreetNumber,
				Locality = address.Address.Town,
				AdminArea = address.Address.Region,
				SubAdminArea = address.Address.District,
				SubLocality = address.Address.Neighborhood
			});
		}
	}
}
