using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.Maps;
using NUnit.Framework;


namespace Microsoft.Maui.Controls.Core.UnitTests
{
	[TestFixture]
	public class GeocoderUnitTests : BaseTestFixture
	{
		[Test]
		public async Task AddressesForPosition()
		{
			Geocoder.GetAddressesForPositionFuncAsync = GetAddressesForPositionFuncAsync;
			var geocoder = new Geocoder();
			var result = await geocoder.GetAddressesForPositionAsync(new Position(1, 2));
			Assert.AreEqual(new String[] { "abc", "def" }, result);
		}

		async Task<IEnumerable<string>> GetAddressesForPositionFuncAsync(Position position)
		{
			Assert.AreEqual(new Position(1, 2), position);
			return new string[] { "abc", "def" };
		}

		[Test]
		public async Task PositionsForAddress()
		{
			Geocoder.GetPositionsForAddressAsyncFunc = GetPositionsForAddressAsyncFunc;
			var geocoder = new Geocoder();
			var result = await geocoder.GetPositionsForAddressAsync("quux");
			Assert.AreEqual(new Position[] { new Position(1, 2), new Position(3, 4) }, result);
		}

		async Task<IEnumerable<Position>> GetPositionsForAddressAsyncFunc(string address)
		{
			Assert.AreEqual("quux", address);
			return new Position[] { new Position(1, 2), new Position(3, 4) };
		}
	}
}
