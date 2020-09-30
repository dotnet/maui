using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contacts;
using CoreLocation;

#if __MOBILE__
using AddressBookUI;
#endif

using CCLGeocoder = CoreLocation.CLGeocoder;

#if __MOBILE__
namespace Xamarin.Forms.Maps.iOS
#else
namespace Xamarin.Forms.Maps.MacOS
#endif
{
	internal class GeocoderBackend
	{
		public static void Register()
		{
			Geocoder.GetPositionsForAddressAsyncFunc = GetPositionsForAddressAsync;
			Geocoder.GetAddressesForPositionFuncAsync = GetAddressesForPositionAsync;
		}

		static Task<IEnumerable<string>> GetAddressesForPositionAsync(Position position)
		{
			var location = new CLLocation(position.Latitude, position.Longitude);
			var geocoder = new CCLGeocoder();
			var source = new TaskCompletionSource<IEnumerable<string>>();
			geocoder.ReverseGeocodeLocation(location, (placemarks, error) =>
			{
				if (placemarks == null)
					placemarks = new CLPlacemark[0];
				List<string> addresses = new List<string>();
#if __MOBILE__
				addresses = placemarks.Select(p => ABAddressFormatting.ToString(p.AddressDictionary, false)).ToList();
#else
				foreach (var item in placemarks)
				{
					var address = new CNMutablePostalAddress();
					address.Street = item.AddressDictionary["Street"] == null ? "" : item.AddressDictionary["Street"].ToString();
					address.State = item.AddressDictionary["State"] == null ? "" : item.AddressDictionary["State"].ToString();
					address.City = item.AddressDictionary["City"] == null ? "" : item.AddressDictionary["City"].ToString();
					address.Country = item.AddressDictionary["Country"] == null ? "" : item.AddressDictionary["Country"].ToString();
					address.PostalCode = item.AddressDictionary["ZIP"] == null ? "" : item.AddressDictionary["ZIP"].ToString();
					addresses.Add(CNPostalAddressFormatter.GetStringFrom(address, CNPostalAddressFormatterStyle.MailingAddress));
				}
#endif
				source.SetResult(addresses);

			});
			return source.Task;
		}

		static Task<IEnumerable<Position>> GetPositionsForAddressAsync(string address)
		{
			var geocoder = new CCLGeocoder();
			var source = new TaskCompletionSource<IEnumerable<Position>>();
			geocoder.GeocodeAddress(address, (placemarks, error) =>
			{
				if (placemarks == null)
					placemarks = new CLPlacemark[0];
				IEnumerable<Position> positions = placemarks.Select(p => new Position(p.Location.Coordinate.Latitude, p.Location.Coordinate.Longitude));
				source.SetResult(positions);
			});
			return source.Task;
		}
	}
}