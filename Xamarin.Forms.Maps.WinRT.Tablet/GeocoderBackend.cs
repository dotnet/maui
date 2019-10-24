using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Bing.Maps;
using Bing.Maps.Search;

namespace Xamarin.Forms.Maps.WinRT
{
	internal class GeocoderBackend
	{
		public static void Register()
		{
			Geocoder.GetPositionsForAddressAsyncFunc = GetPositionsForAddress;
			Geocoder.GetAddressesForPositionFuncAsync = GetAddressesForPositionAsync;
		}

		static async Task<IEnumerable<string>> GetAddressesForPositionAsync(Position position)
		{
			var results = new List<string>();
			var source = new TaskCompletionSource<IEnumerable<string>>();
			var requestOptions = new ReverseGeocodeRequestOptions(new Location(position.Latitude, position.Longitude));
			var response =
				await
					new Bing.Maps.Map { Credentials = FormsMaps.AuthenticationToken }.SearchManager.ReverseGeocodeAsync(requestOptions);
			if (!response.HasError)
			{
				foreach (var address in response.LocationData)
					results.Add(address.Address.FormattedAddress);
			}

			return results;
		}

		static async Task<IEnumerable<Position>> GetPositionsForAddress(string s)
		{
			var results = new List<Position>();

			if (string.IsNullOrEmpty(s))
				return results;

			var requestOptions = new GeocodeRequestOptions(s);
			var response =
				await new Bing.Maps.Map { Credentials = FormsMaps.AuthenticationToken }.SearchManager.GeocodeAsync(requestOptions);
			if (!response.HasError)
			{
				foreach (var address in response.LocationData)
					results.Add(new Position(address.Location.Latitude, address.Location.Longitude));
			}
			return results;
		}
	}
}