using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Devices.Sensors;
using Xunit;

namespace Microsoft.Maui.Essentials.DeviceTests
{
	[Category("Geocoding")]
	public class Geocoding_Tests
	{
		public Geocoding_Tests()
		{
#if WINDOWS_UWP || WINDOWS
			ApplicationModel.Platform.MapServiceToken = "RJHqIE53Onrqons5CNOx~FrDr3XhjDTyEXEjng-CRoA~Aj69MhNManYUKxo6QcwZ0wmXBtyva0zwuHB04rFYAPf7qqGJ5cHb03RCDw1jIW8l";
#endif
		}

		// Temporarily disabling this test on Windows due to consistent CI failures.
		// See https://github.com/dotnet/maui/issues/30507 for tracking re-enablement.
#if !ANDROID && !WINDOWS
		[Theory]
		[InlineData(47.673988, -122.121513)]
		public async Task Get_Placemarks_LatLong(double latitude, double longitude)
		{
			try
			{
				var placemarks = await Geocoding.GetPlacemarksAsync(latitude, longitude);

				Assert.NotNull(placemarks);
				Assert.True(placemarks.Any());
			}
			catch (Exception ex) when (IsEmulatorFailure(ex))
			{
			}
		}

		[Theory]
		[InlineData(47.673988, -122.121513)]
		public async Task Get_Placemarks_Location(double latitude, double longitude)
		{
			try
			{
				var placemarks = await Geocoding.GetPlacemarksAsync(new Location(latitude, longitude));

				Assert.NotNull(placemarks);
				Assert.True(placemarks.Any());
			}
			catch (Exception ex) when (IsEmulatorFailure(ex))
			{
			}
		}

		[Theory]
		[InlineData("Redmond, WA, USA")]
		public async Task Get_Locations(string address)
		{
			try
			{
				var locations = await Geocoding.GetLocationsAsync(address);

				Assert.NotNull(locations);
				Assert.True(locations.Any());
			}
			catch (Exception ex) when (IsEmulatorFailure(ex))
			{
			}
		}
#endif
		static bool IsEmulatorFailure(Exception ex) =>
			ex.Message.Contains("grpc", StringComparison.OrdinalIgnoreCase) || ex.Message.Contains("service not available", StringComparison.OrdinalIgnoreCase);
	}
}
