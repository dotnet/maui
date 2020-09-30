using System.Collections.Generic;
using System.Threading.Tasks;

namespace Xamarin.Forms.Maps.Tizen
{
	internal class GeocoderBackend
	{
		public static void Register()
		{
			Geocoder.GetPositionsForAddressAsyncFunc = GetPositionsForAddressAsync;
			Geocoder.GetAddressesForPositionFuncAsync = GetAddressesForPositionAsync;
		}

		public static async Task<IEnumerable<Position>> GetPositionsForAddressAsync(string address)
		{
			var request = FormsMaps.MapService.CreateGeocodeRequest(address);
			var positions = new List<Position>();
			foreach (var result in await request.GetResponseAsync())
				positions.Add(new Position(result.Latitude, result.Longitude));
			return positions;
		}

		public static async Task<IEnumerable<string>> GetAddressesForPositionAsync(Position position)
		{
			var request = FormsMaps.MapService.CreateReverseGeocodeRequest(position.Latitude, position.Longitude);
			var addresses = new List<string>();
			foreach (var result in await request.GetResponseAsync())
				addresses.Add(result.FreeText);
			return addresses;
		}
	}
}