using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.App;
using Android.Locations;

namespace Microsoft.Maui.Essentials.Implementations
{
	public class GeocodingImplementation : IGeocoding
	{
		public async Task<IEnumerable<Placemark>> GetPlacemarksAsync(double latitude, double longitude)
		{
			using (var geocoder = new Geocoder(Platform.AppContext))
			{
				var addressList = await geocoder.GetFromLocationAsync(latitude, longitude, 10);
				return addressList?.ToPlacemarks();
			}
		}

		public async Task<IEnumerable<Location>> GetLocationsAsync(string address)
		{
			if (address == null)
				throw new ArgumentNullException(nameof(address));

			using (var geocoder = new Geocoder(Platform.AppContext))
			{
				var addressList = await geocoder.GetFromLocationNameAsync(address, 10);

				return addressList?.ToLocations();
			}
		}
	}
}
