using System;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Devices;
using Xunit;

namespace Microsoft.Maui.Essentials.DeviceTests
{
	[Category("DeviceInfo")]
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
		public void Platform_Is_Correct()
		{
#if WINDOWS_UWP || WINDOWS
			Assert.Equal(DevicePlatform.WinUI, DeviceInfo.Platform);
#elif MACCATALYST
			Assert.Equal(DevicePlatform.MacCatalyst, DeviceInfo.Platform);
#elif __IOS__
			Assert.Equal(DevicePlatform.iOS, DeviceInfo.Platform);
#elif __ANDROID__
			Assert.Equal(DevicePlatform.Android, DeviceInfo.Platform);
#else
			throw new PlatformNotSupportedException();
#endif
		}
	}
}
