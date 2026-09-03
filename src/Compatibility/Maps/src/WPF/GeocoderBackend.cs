using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maps.MapControl.WPF;

namespace Microsoft.Maui.Controls.Compatibility.Maps.WPF
{
	internal class GeocoderBackend
	{
		public static void Register()
		{
			Geocoder.GetPositionsForAddressAsyncFunc = GetPositionsForAddress;
			Geocoder.GetAddressesForPositionFuncAsync = GetAddressesForPositionAsync;
		}

		static Task<IEnumerable<string>> GetAddressesForPositionAsync(Position position)
		{
			return Task.FromResult<IEnumerable<string>>(new List<string>());
		}

		static Task<IEnumerable<Position>> GetPositionsForAddress(string address)
		{
			return Task.FromResult<IEnumerable<Position>>(new List<Position>());
		}
	}
}
