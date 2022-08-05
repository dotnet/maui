using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.Maps;
using Xunit;


namespace Microsoft.Maui.Controls.Core.UnitTests
{

	public class GeocoderUnitTests : BaseTestFixture
	{
		[Fact]
		public async Task AddressesForPosition()
		{
			Geocoder.GetAddressesForPositionFuncAsync = GetAddressesForPositionFuncAsync;
			var geocoder = new Geocoder();
			var result = await geocoder.GetAddressesForPositionAsync(new Position(1, 2));
			Assert.Equal(new String[] { "abc", "def" }, result);
		}

		async Task<IEnumerable<string>> GetAddressesForPositionFuncAsync(Position position)
		{
			Assert.Equal(new Position(1, 2), position);
			return new string[] { "abc", "def" };
		}

		[Fact]
		public async Task PositionsForAddress()
		{
			Geocoder.GetPositionsForAddressAsyncFunc = GetPositionsForAddressAsyncFunc;
			var geocoder = new Geocoder();
			var result = await geocoder.GetPositionsForAddressAsync("quux");
			Assert.Equal(new Position[] { new Position(1, 2), new Position(3, 4) }, result);
		}

		async Task<IEnumerable<Position>> GetPositionsForAddressAsyncFunc(string address)
		{
			Assert.Equal("quux", address);
			return new Position[] { new Position(1, 2), new Position(3, 4) };
		}
	}
}
