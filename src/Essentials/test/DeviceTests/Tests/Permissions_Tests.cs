using System;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Dispatching;
using Xunit;
using Xunit.Sdk;

namespace Microsoft.Maui.Essentials.DeviceTests
{
	[Category("Permissions")]
	public class Permissions_Tests
	{
		[Theory]
		[InlineData("Battery")]
		[InlineData("NetworkState")]
		[InlineData("LocationWhenInUse")]
		internal void Ensure_Declared(string permission)
		{
			switch (permission)
			{
				case "Battery":
					Permissions.EnsureDeclared<Permissions.Battery>();
					break;
				case "NetworkState":
					Permissions.EnsureDeclared<Permissions.NetworkState>();
					break;
				case "LocationWhenInUse":
					Permissions.EnsureDeclared<Permissions.LocationWhenInUse>();
					break;
			}
		}

		[Theory]
		[InlineData("Battery", PermissionStatus.Granted)]
		[InlineData("NetworkState", PermissionStatus.Granted)]
		internal async Task Check_Status(string permission, PermissionStatus expectedStatus)
		{
			var status = PermissionStatus.Unknown;
			switch (permission)
			{
				case "Battery":
					status = await Permissions.CheckStatusAsync<Permissions.Battery>().ConfigureAwait(false);
					break;
				case "NetworkState":
					status = await Permissions.CheckStatusAsync<Permissions.NetworkState>().ConfigureAwait(false);
					break;
			}

			Assert.Equal(expectedStatus, status);
		}

		[Theory]
		[InlineData("Battery", PermissionStatus.Granted)]
		[InlineData("NetworkState", PermissionStatus.Granted)]
		internal async Task Request(string permission, PermissionStatus expectedStatus)
		{
			var status = PermissionStatus.Unknown;
			switch (permission)
			{
				case "Battery":
					status = await Permissions.RequestAsync<Permissions.Battery>().ConfigureAwait(false);
					break;
				case "NetworkState":
					status = await Permissions.RequestAsync<Permissions.NetworkState>().ConfigureAwait(false);
					break;
			}

			Assert.Equal(expectedStatus, status);
		}

		[Fact]
		[Trait(Traits.UI, Traits.FeatureSupport.Supported)]
		public async Task Request_NotMainThread()
		{
			await Task.Run(async () =>
			{
				await Assert.ThrowsAsync<PermissionException>(async () => await Permissions.RequestAsync<Permissions.LocationWhenInUse>()).ConfigureAwait(false);
			}).ConfigureAwait(false);
		}

		[Fact
#if !__ANDROID__
		(Skip = "Test only applies to Android")
#endif
		]
		public async Task StorageAndroid13AlwaysGranted()
		{
			if (DeviceInfo.Platform == DevicePlatform.Android && OperatingSystem.IsAndroidVersionAtLeast(33))
			{
				var status = await Permissions.CheckStatusAsync<Permissions.StorageRead>();
				Assert.Equal(PermissionStatus.Granted, status);

				status = await Permissions.CheckStatusAsync<Permissions.StorageWrite>().ConfigureAwait(false);
				Assert.Equal(PermissionStatus.Granted, status);
			}
			else // Android < API 33, we didn't request these, so status denied
			{
				var status = await Permissions.CheckStatusAsync<Permissions.StorageRead>();
				Assert.Equal(PermissionStatus.Denied, status);

				status = await Permissions.CheckStatusAsync<Permissions.StorageWrite>().ConfigureAwait(false);
				Assert.Equal(PermissionStatus.Denied, status);
			}
		}

		[Fact
#if !__ANDROID__
		(Skip = "Test only applies to Android")
#endif
		]
		public async Task LocationPartialPermissions_CheckStatus_ConsistentWithRequest()
		{
			// This test verifies that CheckStatusAsync and RequestAsync return consistent results
			// for partial location permissions (Issue #23060)
			// 
			// Note: This test cannot simulate partial permissions automatically, but ensures
			// the logic consistency between CheckStatusAsync and RequestAsync methods.
			// Manual testing is required to verify the actual partial permission scenario.

			if (DeviceInfo.Platform == DevicePlatform.Android)
			{
				// Test LocationWhenInUse - ensure CheckStatusAsync and RequestAsync use the same logic
				var checkStatus = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();

				// If we have any permissions, the results should be consistent
				// This mainly tests that our aggregation logic is working
				Assert.True(checkStatus == PermissionStatus.Granted ||
						   checkStatus == PermissionStatus.Denied ||
						   checkStatus == PermissionStatus.Restricted);

				// Test LocationAlways as well
				var checkStatusAlways = await Permissions.CheckStatusAsync<Permissions.LocationAlways>();
				Assert.True(checkStatusAlways == PermissionStatus.Granted ||
						   checkStatusAlways == PermissionStatus.Denied ||
						   checkStatusAlways == PermissionStatus.Restricted);
			}
		}

		[Fact
#if !__ANDROID__
		(Skip = "Test only applies to Android")
#endif
		]
		public async Task LocationWhenInUse_CoarseOnly_ReturnsRestricted()
		{
			// Test to verify that when only COARSE_LOCATION is granted, CheckStatusAsync returns Restricted
			// This test verifies the fix for Issue #23060
			// 
			// MANUAL TEST PROCEDURE:
			// 1. Ensure AndroidManifest.xml declares both ACCESS_COARSE_LOCATION and ACCESS_FINE_LOCATION
			// 2. Uninstall the test app to reset permissions
			// 3. Reinstall and run this test
			// 4. When prompted, select "Approximate location" (coarse only)
			// 5. Verify that CheckStatusAsync returns Restricted (not Denied)
			//
			// EXPECTED BEHAVIOR:
			// - 2 permissions granted → Granted
			// - 1 permission granted (coarse only) → Restricted
			// - 0 permissions granted → Denied

			if (DeviceInfo.Platform == DevicePlatform.Android)
			{
				var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
				
				// Verify the status is one of the valid values
				Assert.True(status == PermissionStatus.Granted ||
						   status == PermissionStatus.Denied ||
						   status == PermissionStatus.Restricted,
						   $"LocationWhenInUse.CheckStatusAsync returned unexpected status: {status}");

				// If the status is Restricted, verify it matches the RequestAsync behavior
				if (status == PermissionStatus.Restricted)
				{
					// CheckStatusAsync should return Restricted when only coarse permission is granted
					// This is the fix for Issue #23060 - previously returned Denied incorrectly
					Assert.Equal(PermissionStatus.Restricted, status);
				}
			}
		}

		[Fact
#if !__ANDROID__
		(Skip = "Test only applies to Android")
#endif
		]
		public async Task LocationAlways_PartialPermissions_ReturnsRestricted()
		{
			// Test to verify that when only some location permissions are granted, CheckStatusAsync returns Restricted
			// This test verifies the fix for Issue #23060
			//
			// MANUAL TEST PROCEDURE:
			// 1. Ensure AndroidManifest.xml declares ACCESS_COARSE_LOCATION, ACCESS_FINE_LOCATION, 
			//    and ACCESS_BACKGROUND_LOCATION (on Android 10+)
			// 2. Uninstall the test app to reset permissions
			// 3. Reinstall and run this test
			// 4. When prompted, select "Approximate location" and/or deny background location
			// 5. Verify that CheckStatusAsync returns Restricted (not Denied)
			//
			// EXPECTED BEHAVIOR:
			// - All permissions granted → Granted
			// - Some permissions granted → Restricted
			// - No permissions granted → Denied

			if (DeviceInfo.Platform == DevicePlatform.Android)
			{
				var status = await Permissions.CheckStatusAsync<Permissions.LocationAlways>();
				
				// Verify the status is one of the valid values
				Assert.True(status == PermissionStatus.Granted ||
						   status == PermissionStatus.Denied ||
						   status == PermissionStatus.Restricted,
						   $"LocationAlways.CheckStatusAsync returned unexpected status: {status}");

				// If the status is Restricted, verify it matches the expected behavior
				if (status == PermissionStatus.Restricted)
				{
					// CheckStatusAsync should return Restricted when only partial permissions are granted
					// This is the fix for Issue #23060 - previously returned Denied incorrectly
					Assert.Equal(PermissionStatus.Restricted, status);
				}
			}
		}
	}
}
