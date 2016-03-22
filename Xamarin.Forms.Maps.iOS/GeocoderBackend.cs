using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
#if __UNIFIED__
using CoreLocation;
using AddressBookUI;
#else
using MonoTouch.AddressBookUI;
using MonoTouch.CoreLocation;
#endif
#if __UNIFIED__
using RectangleF = CoreGraphics.CGRect;
using SizeF = CoreGraphics.CGSize;
using PointF = CoreGraphics.CGPoint;

#else
using nfloat=global::System.Single;
using nint=global::System.Int32;
using nuint=global::System.UInt32;
#endif

namespace Xamarin.Forms.Maps.iOS
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
			var geocoder = new CLGeocoder();
			var source = new TaskCompletionSource<IEnumerable<string>>();
			geocoder.ReverseGeocodeLocation(location, (placemarks, error) =>
			{
				if (placemarks == null)
					placemarks = new CLPlacemark[0];
				IEnumerable<string> addresses = placemarks.Select(p => ABAddressFormatting.ToString(p.AddressDictionary, false));
				source.SetResult(addresses);
			});
			return source.Task;
		}

		static Task<IEnumerable<Position>> GetPositionsForAddressAsync(string address)
		{
			var geocoder = new CLGeocoder();
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