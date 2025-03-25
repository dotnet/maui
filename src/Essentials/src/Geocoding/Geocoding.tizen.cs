#pragma warning disable CS0618 // Type or member is obsolete
#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;

namespace Microsoft.Maui.Devices.Sensors
{
	class GeocodingImplementation : IGeocoding
	{
		public string? MapServiceToken { get; set; }

		public async Task<IEnumerable<Placemark>> GetPlacemarksAsync(double latitude, double longitude)
		{
			ValidateMapServiceToken();

			Permissions.EnsureDeclared<Permissions.Maps>();

			var map = await PlatformUtils.GetMapServiceAsync(MapServiceToken);
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

		public async Task<IEnumerable<Location>> GetLocationsAsync(string address)
		{
			ValidateMapServiceToken();

			Permissions.EnsureDeclared<Permissions.Maps>();

			var map = await PlatformUtils.GetMapServiceAsync(MapServiceToken);
			var request = map.CreateGeocodeRequest(address);
			var list = new List<Location>();
			foreach (var position in await request.GetResponseAsync())
				list.Add(new Location(position.Latitude, position.Longitude));
			return list;
		}

		void ValidateMapServiceToken()
		{
			if (string.IsNullOrWhiteSpace(MapServiceToken) && string.IsNullOrWhiteSpace(MapServiceToken))
				throw new ArgumentNullException(nameof(MapServiceToken), "Please set the map service token to be able to use this API.");
		}
	}
}
