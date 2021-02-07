using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Xamarin.Essentials
{
	public static partial class Geocoding
	{
		static async Task<IEnumerable<Placemark>> PlatformGetPlacemarksAsync(double latitude, double longitude)
		{
			Permissions.EnsureDeclared<Permissions.Maps>();
			if (string.IsNullOrWhiteSpace(Platform.MapServiceToken))
				throw new ArgumentNullException(nameof(Platform.MapServiceToken));
			var map = await Platform.GetMapServiceAsync(Platform.MapServiceToken);
			var request = map.CreateReverseGeocodeRequest(latitude, longitude);

			var list = new List<Placemark>();
			foreach (var address in await request.GetResponseAsync())
			{
				list.Add(new Placemark
				{
					CountryCode = address.CountryCode,
					CountryName = address.Country,
					AdminArea = address.State,
					SubAdminArea = address.County,
					Locality = address.City,
					SubLocality = address.District,
					Thoroughfare = address.Street,
					SubThoroughfare = address.Building,
					FeatureName = address.Street,
					Location = new Location(latitude, longitude),
					PostalCode = address.PostalCode,
				});
			}
			return list;
		}

		static async Task<IEnumerable<Location>> PlatformGetLocationsAsync(string address)
		{
			Permissions.EnsureDeclared<Permissions.Maps>();
			if (string.IsNullOrWhiteSpace(Platform.MapServiceToken))
				throw new ArgumentNullException(nameof(Platform.MapServiceToken));
			var map = await Platform.GetMapServiceAsync(Platform.MapServiceToken);
			var request = map.CreateGeocodeRequest(address);
			var list = new List<Location>();
			foreach (var position in await request.GetResponseAsync())
				list.Add(new Location(position.Latitude, position.Longitude));
			return list;
		}
	}
}
