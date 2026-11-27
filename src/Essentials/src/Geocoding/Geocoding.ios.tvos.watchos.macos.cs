#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CoreLocation;

namespace Microsoft.Maui.Devices.Sensors
{
	class GeocodingImplementation : IGeocoding
	{
		public async Task<IEnumerable<Placemark>> GetPlacemarksAsync(double latitude, double longitude)
		{
#pragma warning disable CA1422 // CLGeocoder is obsolete on Mac Catalyst 26+; migrating to MapKit is tracked separately
			using (var geocoder = new CLGeocoder())
			{
				var addressList = await geocoder.ReverseGeocodeLocationAsync(new CLLocation(latitude, longitude));
#pragma warning restore CA1422

				return addressList?.ToPlacemarks() ?? Array.Empty<Placemark>();
			}
		}

		public async Task<IEnumerable<Location>> GetLocationsAsync(string address)
		{
#pragma warning disable CA1422 // CLGeocoder is obsolete on Mac Catalyst 26+; migrating to MapKit is tracked separately
			using (var geocoder = new CLGeocoder())
			{
				var positionList = await geocoder.GeocodeAddressAsync(address);
#pragma warning restore CA1422

				return positionList?.ToLocations() ?? Array.Empty<Location>();
			}
		}
	}
}
