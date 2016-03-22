using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.Content;
using Android.Locations;
using AGeocoder = Android.Locations.Geocoder;

namespace Xamarin.Forms.Maps.Android
{
	internal class GeocoderBackend
	{
		public static void Register(Context context)
		{
			Geocoder.GetPositionsForAddressAsyncFunc = GetPositionsForAddressAsync;
			Geocoder.GetAddressesForPositionFuncAsync = GetAddressesForPositionAsync;
		}

		public static async Task<IEnumerable<Position>> GetPositionsForAddressAsync(string address)
		{
			var geocoder = new AGeocoder(Forms.Context);
			IList<Address> addresses = await geocoder.GetFromLocationNameAsync(address, 5);
			return addresses.Select(p => new Position(p.Latitude, p.Longitude));
		}

		public static async Task<IEnumerable<string>> GetAddressesForPositionAsync(Position position)
		{
			var geocoder = new AGeocoder(Forms.Context);
			IList<Address> addresses = await geocoder.GetFromLocationAsync(position.Latitude, position.Longitude, 5);
			return addresses.Select(p =>
			{
				IEnumerable<string> lines = Enumerable.Range(0, p.MaxAddressLineIndex + 1).Select(p.GetAddressLine);
				return string.Join("\n", lines);
			});
		}
	}
}