using System.Collections.Generic;
using System.Threading.Tasks;
using CoreLocation;

namespace Microsoft.Maui.Essentials.Implementations
{
	public class GeocodingImplementation : IGeocoding
	{
		public async Task<IEnumerable<Placemark>> GetPlacemarksAsync(double latitude, double longitude)
		{
			using (var geocoder = new CLGeocoder())
			{
				var addressList = await geocoder.ReverseGeocodeLocationAsync(new CLLocation(latitude, longitude));

				return addressList?.ToPlacemarks();
			}
		}

		public async Task<IEnumerable<Location>> GetLocationsAsync(string address)
		{
			using (var geocoder = new CLGeocoder())
			{
				var positionList = await geocoder.GeocodeAddressAsync(address);

				return positionList?.ToLocations();
			}
		}
	}
}
