using System;
using System.Threading.Tasks;
using Microsoft.Maui.Essentials;
using Xunit;

namespace Microsoft.Maui.Essentials.DeviceTests
{
	public class Maps_Tests
	{
		const double testLatitude = 47.645160;
		const double testLongitude = -122.1306032;
		const string mapName = "Microsoft Building 25";

		[Fact]
		[Trait(Traits.InteractionType, Traits.InteractionTypes.Human)]
		public async Task LaunchMap_CoordinatesDisplayCorrectPlace()
		{
			await Map.OpenAsync(testLatitude, testLongitude, new MapLaunchOptions { Name = mapName });
		}

		[Fact]
		[Trait(Traits.InteractionType, Traits.InteractionTypes.Human)]
		public async Task LaunchMap_PlacemarkDisplayCorrectPlace()
		{
			var placemark = new Placemark
			{
				CountryName = "United States",
				AdminArea = "WA",
				Thoroughfare = "Microsoft Building 25",
				Locality = "Redmond"
			};
			await Map.OpenAsync(placemark, new MapLaunchOptions { Name = mapName });
		}

		[Fact]
		public async Task LaunchMap_NullLocation()
		{
			Location location = null;
			await Assert.ThrowsAsync<ArgumentNullException>(() => Map.OpenAsync(location));
		}

		[Fact]
		public async Task LaunchMap_NullOptionsLocation()
		{
			var location = new Location(testLatitude, testLongitude);
			await Assert.ThrowsAsync<ArgumentNullException>(() => Map.OpenAsync(location, null));
		}

		[Fact]
		public async Task LaunchMap_NullPlacemark()
		{
			Placemark location = null;
			await Assert.ThrowsAsync<ArgumentNullException>(() => Map.OpenAsync(location));
		}

		[Fact]
		public async Task LaunchMap_NullOptionsPlacemark()
		{
			var placemark = new Placemark
			{
				CountryName = "United States",
				AdminArea = "WA",
				Thoroughfare = "Microsoft Building 25",
				Locality = "Redmond"
			};
			await Assert.ThrowsAsync<ArgumentNullException>(() => Map.OpenAsync(placemark, null));
		}
	}
}
