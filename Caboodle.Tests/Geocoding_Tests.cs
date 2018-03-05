using System.Threading.Tasks;
using Microsoft.Caboodle;
using Xunit;

namespace Caboodle.Tests
{
    public class Geocoding_Tests
    {
        [Fact]
        public async Task Geocoding_Placemarks_Fail_On_NetStandard()
        {
            await Assert.ThrowsAsync<NotImplentedInReferenceAssembly>(() => Geocoding.GetPlacemarksAsync(1, 1));
        }

        [Fact]
        public async Task Geocoding_Placemarks_Location_Fail_On_NetStandard()
        {
            await Assert.ThrowsAsync<NotImplentedInReferenceAssembly>(() => Geocoding.GetPlacemarksAsync(new Location(1, 1)));
        }

        [Fact]
        public async Task Geocoding_Locations_On_NetStandard()
        {
            await Assert.ThrowsAsync<NotImplentedInReferenceAssembly>(() => Geocoding.GetLocationsAsync("Microsoft Building 25"));
        }
    }
}
