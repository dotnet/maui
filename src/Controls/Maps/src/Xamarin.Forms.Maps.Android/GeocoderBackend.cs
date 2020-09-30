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
		readonly Context _context;

		AGeocoder _geocoder;
		AGeocoder AndroidGeocoder => _geocoder ?? (_geocoder = new AGeocoder(_context));

		public GeocoderBackend(Context context)
		{
			_context = context;
		}

		public void Register()
		{
			Geocoder.GetPositionsForAddressAsyncFunc = GetPositionsForAddressAsync;
			Geocoder.GetAddressesForPositionFuncAsync = GetAddressesForPositionAsync;
		}

		public async Task<IEnumerable<Position>> GetPositionsForAddressAsync(string address)
		{
			IList<Address> addresses = await AndroidGeocoder.GetFromLocationNameAsync(address, 5);
			return addresses.Select(p => new Position(p.Latitude, p.Longitude));
		}

		public async Task<IEnumerable<string>> GetAddressesForPositionAsync(Position position)
		{
			IList<Address> addresses = await AndroidGeocoder.GetFromLocationAsync(position.Latitude, position.Longitude, 5);
			return addresses.Select(p =>
			{
				IEnumerable<string> lines = Enumerable.Range(0, p.MaxAddressLineIndex + 1).Select(p.GetAddressLine);
				return string.Join("\n", lines);
			});
		}
	}
}