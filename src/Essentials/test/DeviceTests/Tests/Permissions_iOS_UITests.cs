using System;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Dispatching;
using Xunit;

namespace Microsoft.Maui.Essentials.DeviceTests
{
	// These tests are specifically designed to test the UI behavior of iOS permissions
	[Category("iOS")]
	[Category("Permissions")]
	[Category("UITest")]
	public class Permissions_iOS_UITests
	{
#if __IOS__
		[Fact]
		public async Task Location_Permission_UI_Shows_On_Request()
		{
			// Skip test on non-iOS platforms
			if (DeviceInfo.Platform != DevicePlatform.iOS && DeviceInfo.Platform != DevicePlatform.MacCatalyst)
				return;

			// Run the permission request on a background thread
			var permissionTask = Task.Run(async () =>
			{
				return await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
			});

			// The system should show the permission dialog when requested
			// We can only verify this runs without exceptions since we can't programmatically
			// interact with system permission dialogs in automated tests
			var status = await permissionTask;
			
			// Ensure we got a valid response
			Assert.NotEqual(PermissionStatus.Unknown, status);
		}

		[Fact]
		public async Task Location_Always_Permission_UI_Shows_On_Request()
		{
			// Skip test on non-iOS platforms
			if (DeviceInfo.Platform != DevicePlatform.iOS && DeviceInfo.Platform != DevicePlatform.MacCatalyst)
				return;

			// First request "when in use" permission which is required before "always" permission
			await Permissions.RequestAsync<Permissions.LocationWhenInUse>();

			// Run the "always" permission request on a background thread
			var permissionTask = Task.Run(async () =>
			{
				return await Permissions.RequestAsync<Permissions.LocationAlways>();
			});

			// The system should show the permission dialog when requested
			// We can only verify this runs without exceptions since we can't programmatically
			// interact with system permission dialogs in automated tests
			var status = await permissionTask;
			
			// Ensure we got a valid response
			Assert.NotEqual(PermissionStatus.Unknown, status);
		}
#endif
	}
}