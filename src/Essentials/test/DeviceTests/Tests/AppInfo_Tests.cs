using System;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Devices;
using Xunit;

namespace Microsoft.Maui.Essentials.DeviceTests
{
	[Category("AppInfo")]
	public class AppInfo_Tests
	{
		[Fact]
		public void AppName_Is_Correct()
		{
			Assert.Equal("Essentials Tests", AppInfo.Name);
		}

		[Fact]
		public void AppPackageName_Is_Correct()
		{
#if WINDOWS_UWP || WINDOWS || __IOS__ || __ANDROID__
			Assert.Equal("com.microsoft.maui.essentials.devicetests", AppInfo.PackageName);
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
#if WINDOWS_UWP || WINDOWS
			Assert.Equal("1.0.0.1", AppInfo.VersionString);
			Assert.Equal(new Version(1, 0, 0, 1), AppInfo.Version);
#else
			Assert.Equal("1.0", AppInfo.VersionString);
			Assert.Equal(new Version(1, 0), AppInfo.Version);
#endif
		}
	}
}
