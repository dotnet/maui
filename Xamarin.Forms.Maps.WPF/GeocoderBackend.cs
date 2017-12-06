using Microsoft.Maps.MapControl.WPF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xamarin.Forms.Maps.WPF
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
			return new List<string>();
		}

		static async Task<IEnumerable<Position>> GetPositionsForAddress(string address)
		{
			return new List<Position>();
		}
	}
}
