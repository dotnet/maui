using System;
using System.Threading.Tasks;
using Microsoft.Maui.Essentials;
using Xunit;

namespace Microsoft.Maui.Essentials.DeviceTests
{
	public class DeviceInfo_Tests
	{
		[Fact]
		public void Versions_Are_Correct()
		{
#if WINDOWS_UWP || WINDOWS
			Assert.Equal(10, DeviceInfo.Version.Major);
			Assert.Equal(0, DeviceInfo.Version.Minor);
			Assert.StartsWith("10.0", DeviceInfo.VersionString, StringComparison.Ordinal);
#else
			Assert.True(DeviceInfo.Version.Major > 0);
#endif
		}

		[Fact]
		public void AppName_Is_Correct()
		{
			Assert.Equal("Essentials Tests", AppInfo.Name);
		}

		[Fact]
		public void DeviceModel_Is_Correct()
		{
#if WINDOWS_UWP || WINDOWS
			// Nothing right now.
#elif __IOS__
			if (DeviceInfo.DeviceType == DeviceType.Virtual)
			{
				Assert.Equal("x86_64", DeviceInfo.Model);
			}
#elif __ANDROID__

			if (DeviceInfo.DeviceType == DeviceType.Virtual)
			{
				var isEmulator =
					DeviceInfo.Model.Contains("sdk_gphone_x86", StringComparison.Ordinal) ||
					DeviceInfo.Model.Contains("google_sdk", StringComparison.Ordinal) ||
					DeviceInfo.Model.Contains("Emulator", StringComparison.Ordinal) ||
					DeviceInfo.Model.Contains("Android SDK built for x86", StringComparison.Ordinal);

				Assert.True(isEmulator);
			}
#else
			throw new PlatformNotSupportedException();
#endif
		}

		[Fact]
		public void AppPackageName_Is_Correct()
		{
#if WINDOWS_UWP || WINDOWS
			Assert.Equal("ec0cc741-fd3e-485c-81be-68815c480690", AppInfo.PackageName);
#elif __IOS__
			Assert.Equal("com.microsoft.maui.essentials.devicetests", AppInfo.PackageName);
#elif __ANDROID__
			Assert.Equal("com.microsoft.maui.essentials.devicetests", AppInfo.PackageName);
#else
			throw new PlatformNotSupportedException();
#endif
		}

		[Fact]
		public void Platform_Is_Correct()
		{
#if WINDOWS_UWP || WINDOWS
			Assert.Equal(DevicePlatform.WinUI, DeviceInfo.Platform);
#elif __IOS__
			Assert.Equal(DevicePlatform.iOS, DeviceInfo.Platform);
#elif __ANDROID__
			Assert.Equal(DevicePlatform.Android, DeviceInfo.Platform);
#else
			throw new PlatformNotSupportedException();
#endif
		}

		[Fact]
		public void App_Theme_Is_Correct()
		{
#if __IOS__
			if (DeviceInfo.Version.Major >= 13)
				Assert.NotEqual(AppTheme.Unspecified, AppInfo.RequestedTheme);
			else
				Assert.Equal(AppTheme.Unspecified, AppInfo.RequestedTheme);
#elif WINDOWS_UWP || WINDOWS || __ANDROID__
			Assert.NotEqual(AppTheme.Unspecified, AppInfo.RequestedTheme);
#else
			Assert.Equal(AppTheme.Unspecified, AppInfo.RequestedTheme);
#endif
		}

		[Fact]
		public void App_Build_Is_Correct()
		{
			Assert.Equal("1", AppInfo.BuildString);
		}

		[Fact]
		public void App_RequestedLayoutDirection_Is_Correct()
		{
			Assert.Equal(LayoutDirection.LeftToRight, AppInfo.RequestedLayoutDirection);
		}

		[Fact]
		public void App_Versions_Are_Correct()
		{
			Assert.Equal("1.0.1.0", AppInfo.VersionString);
			Assert.Equal(new Version(1, 0, 1, 0), AppInfo.Version);
		}

		[Fact]
		public Task Screen_Metrics_Are_Not_Null()
		{
			return Utils.OnMainThread(() =>
			{
				var metrics = DeviceDisplay.MainDisplayInfo;

				Assert.True(metrics.Width > 0);
				Assert.True(metrics.Height > 0);
				Assert.True(metrics.Density > 0);
			});
		}

		[Fact]
		[Trait(Traits.UI, Traits.FeatureSupport.Supported)]
		public Task ScreenLock_Locks()
		{
			return Utils.OnMainThread(() =>
			{
				Assert.False(DeviceDisplay.KeepScreenOn);

				DeviceDisplay.KeepScreenOn = true;
				Assert.True(DeviceDisplay.KeepScreenOn);

				DeviceDisplay.KeepScreenOn = false;
				Assert.False(DeviceDisplay.KeepScreenOn);
			});
		}

		[Fact]
		[Trait(Traits.UI, Traits.FeatureSupport.Supported)]
		public Task ScreenLock_Unlocks_Without_Locking()
		{
			return Utils.OnMainThread(() =>
			{
				Assert.False(DeviceDisplay.KeepScreenOn);

				DeviceDisplay.KeepScreenOn = false;
				Assert.False(DeviceDisplay.KeepScreenOn);
			});
		}

		[Fact]
		[Trait(Traits.UI, Traits.FeatureSupport.Supported)]
		public Task ScreenLock_Locks_Only_Once()
		{
			return Utils.OnMainThread(() =>
			{
				Assert.False(DeviceDisplay.KeepScreenOn);

				DeviceDisplay.KeepScreenOn = true;
				Assert.True(DeviceDisplay.KeepScreenOn);
				DeviceDisplay.KeepScreenOn = true;
				Assert.True(DeviceDisplay.KeepScreenOn);

				DeviceDisplay.KeepScreenOn = false;
				Assert.False(DeviceDisplay.KeepScreenOn);
			});
		}
	}
}
