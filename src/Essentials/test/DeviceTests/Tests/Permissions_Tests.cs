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
	}
}
