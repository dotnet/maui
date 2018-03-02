using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.App;
using Android.Locations;

namespace Microsoft.Caboodle
{
    public partial class Geocoding
    {
        public static async Task<IEnumerable<Placemark>> GetPlacemarksAsync(double latitude, double longitude)
        {
            using (var geocoder = new Geocoder(Application.Context))
            {
                var addressList = await geocoder.GetFromLocationAsync(latitude, longitude, 10);
                return addressList?.ToPlacemarks();
            }
        }

        public static async Task<IEnumerable<Location>> GetLocationsAsync(string address)
        {
            if (address == null)
                throw new ArgumentNullException(nameof(address));

            using (var geocoder = new Geocoder(Application.Context))
            {
                var addressList = await geocoder.GetFromLocationNameAsync(address, 10);

                return addressList?.ToLocations();
            }
        }
    }
}
