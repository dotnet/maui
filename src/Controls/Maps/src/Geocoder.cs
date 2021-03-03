using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Microsoft.Maui.Controls.Maps
{
	public class Geocoder
	{
		[EditorBrowsable(EditorBrowsableState.Never)]
		public static Func<string, Task<IEnumerable<Position>>> GetPositionsForAddressAsyncFunc;

		[EditorBrowsable(EditorBrowsableState.Never)]
		public static Func<Position, Task<IEnumerable<string>>> GetAddressesForPositionFuncAsync;

		public Task<IEnumerable<string>> GetAddressesForPositionAsync(Position position)
		{
			if (GetAddressesForPositionFuncAsync == null)
				throw new InvalidOperationException("You MUST call Microsoft.Maui.ControlsMaps.Init (); prior to using it.");
			return GetAddressesForPositionFuncAsync(position);
		}

		public Task<IEnumerable<Position>> GetPositionsForAddressAsync(string address)
		{
			if (GetPositionsForAddressAsyncFunc == null)
				throw new InvalidOperationException("You MUST call Microsoft.Maui.ControlsMaps.Init (); prior to using it.");
			return GetPositionsForAddressAsyncFunc(address);
		}
	}
}