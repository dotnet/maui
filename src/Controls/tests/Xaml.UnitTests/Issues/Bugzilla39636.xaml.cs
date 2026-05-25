using System;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Devices;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Bugzilla39636 : ContentPage
{
	[Collection("Issue")]
	public class Tests : IDisposable
	{
		MockDeviceInfo mockDeviceInfo;

		public Tests()
		{
			Application.SetCurrentApplication(new MockApplication());
			DeviceInfo.SetCurrent(mockDeviceInfo = new MockDeviceInfo());
		}

		public void Dispose() => DeviceInfo.SetCurrent(null);

		[Theory]
		[XamlInflatorData]
		internal void OnPlatformWithMissingTargetPlatformShouldUseDefault(XamlInflator inflator)
		{
			// Reproduces Bugzilla39636: When MacCatalyst is not defined in OnPlatform,
			// all inflators should use default(T) instead of throwing an exception

			// Test with MacCatalyst where platform is not defined
			mockDeviceInfo.Platform = DevicePlatform.MacCatalyst;
			var page = new Bugzilla39636(inflator);

			// Should use default value (0.0 for double) instead of throwing
			Assert.NotNull(page.testLabel);
			Assert.Equal(0.0, page.testLabel.WidthRequest);
		}
	}
}

