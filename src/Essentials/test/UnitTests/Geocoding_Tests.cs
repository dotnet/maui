using System.Threading.Tasks;
using Xamarin.Essentials;
using Xunit;

namespace Tests
{
	public class Geocoding_Tests
	{
		[Fact]
		public async Task Geocoding_Placemarks_Fail_On_NetStandard()
		{
			await Assert.ThrowsAsync<NotImplementedInReferenceAssemblyException>(() => Geocoding.GetPlacemarksAsync(1, 1));
		}

		[Fact]
		public async Task Geocoding_Placemarks_Location_Fail_On_NetStandard()
		{
			await Assert.ThrowsAsync<NotImplementedInReferenceAssemblyException>(() => Geocoding.GetPlacemarksAsync(new Location(1, 1)));
		}

		[Fact]
		public async Task Geocoding_Locations_On_NetStandard()
		{
			await Assert.ThrowsAsync<NotImplementedInReferenceAssemblyException>(() => Geocoding.GetLocationsAsync("Microsoft Building 25"));
		}
	}
}
