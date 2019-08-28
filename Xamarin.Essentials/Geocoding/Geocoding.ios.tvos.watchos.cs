using System.Collections.Generic;
using System.Threading.Tasks;
using CoreLocation;

namespace Xamarin.Essentials
{
    public static partial class Geocoding
    {
        static async Task<IEnumerable<Placemark>> PlatformGetPlacemarksAsync(double latitude, double longitude)
        {
            using (var geocoder = new CLGeocoder())
            {
                var addressList = await geocoder.ReverseGeocodeLocationAsync(new CLLocation(latitude, longitude));

                return addressList?.ToPlacemarks();
            }
        }

        static async Task<IEnumerable<Location>> PlatformGetLocationsAsync(string address)
        {
            using (var geocoder = new CLGeocoder())
            {
                var positionList = await geocoder.GeocodeAddressAsync(address);

                return positionList?.ToLocations();
            }
        }
    }
}
