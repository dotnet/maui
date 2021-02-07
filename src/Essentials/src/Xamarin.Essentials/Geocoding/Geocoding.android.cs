using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.App;
using Android.Locations;

namespace Xamarin.Essentials
{
	public static partial class Geocoding
	{
		static async Task<IEnumerable<Placemark>> PlatformGetPlacemarksAsync(double latitude, double longitude)
		{
			using (var geocoder = new Geocoder(Platform.AppContext))
			{
				var addressList = await geocoder.GetFromLocationAsync(latitude, longitude, 10);
				return addressList?.ToPlacemarks();
			}
		}

		static async Task<IEnumerable<Location>> PlatformGetLocationsAsync(string address)
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
