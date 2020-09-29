using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GMap.NET;
using GMap.NET.MapProviders;

namespace Xamarin.Forms.Maps.GTK
{
	internal class GeocoderBackend
	{
		public static void Register()
		{
			Geocoder.GetPositionsForAddressAsyncFunc = GetPositionsForAddressAsync;
			Geocoder.GetAddressesForPositionFuncAsync = GetAddressesForPositionAsync;
		}

		public static Task<IEnumerable<Position>> GetPositionsForAddressAsync(string address)
		{
			var points = new List<PointLatLng>();
			var positions = new List<Position>();
			GMapProviders.GoogleMap.GetPoints(address, out points);

			if (points != null && points.Any())
			{
				foreach (var point in points)
				{
					positions.Add(new Position(point.Lat, point.Lng));
				}
			}

			return Task.FromResult<IEnumerable<Position>>(positions);
		}

		public static Task<IEnumerable<string>> GetAddressesForPositionAsync(Position position)
		{
			var point = new PointLatLng(position.Latitude, position.Longitude);
			var direction = new GDirections();
			var addresses = new List<string>();
			GMapProviders.GoogleMap.GetDirections(out direction, point, point, false, true, false, false, true);

			if (!string.IsNullOrEmpty(direction.StartAddress))
			{
				addresses.Add(direction.StartAddress);
			}

			return Task.FromResult<IEnumerable<string>>(addresses);
		}
	}
}