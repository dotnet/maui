using System;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xunit;

namespace Tests
{
    public class Maps_Tests
    {
        const double testLatitude = 47.645160;
        const double testLongitude = -122.1306032;
        const string mapName = "Microsoft Building 25";

        [Fact]
        public async Task Open_Map_LatLong_NetStandard() =>
          await Assert.ThrowsAsync<NotImplementedInReferenceAssemblyException>(
              () => Map.OpenAsync(
                  testLatitude,
                  testLongitude,
                  new MapLaunchOptions { Name = mapName }));

        [Fact]
        public async Task Open_Map_Location_NetStandard() =>
          await Assert.ThrowsAsync<NotImplementedInReferenceAssemblyException>(
              () => Map.OpenAsync(
                  new Location(testLatitude, testLongitude),
                  new MapLaunchOptions { Name = mapName }));

        [Fact]
        public async Task Open_Map_Placemark_NetStandard() =>
          await Assert.ThrowsAsync<NotImplementedInReferenceAssemblyException>(
              () => Map.OpenAsync(
                  new Placemark(),
                  new MapLaunchOptions { Name = mapName }));

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
