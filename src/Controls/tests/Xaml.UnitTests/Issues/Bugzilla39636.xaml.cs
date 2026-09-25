using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Devices;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Bugzilla39636 : ContentPage
{
	[Collection("Issue")]
	public class Tests : BaseTestFixture
	{
		MockDeviceInfo mockDeviceInfo;

		public Tests()
		{
			Application.SetCurrentApplication(new MockApplication());
			DeviceInfo.SetCurrent(mockDeviceInfo = new MockDeviceInfo());
		}

		public static IEnumerable<object[]> Platforms
		{
			get
			{
				foreach (var inflator in Enum.GetValues<XamlInflator>())
				{
					yield return new object[] { inflator, DevicePlatform.iOS, 40.0 };
					yield return new object[] { inflator, DevicePlatform.Android, 30.0 };
					yield return new object[] { inflator, DevicePlatform.MacCatalyst, 0.0 };
					yield return new object[] { inflator, DevicePlatform.WinUI, 60.0 };
				}
			}
		}

		[Theory]
		[MemberData(nameof(Platforms))]
		internal void ResourceAndInlineOnPlatformUseTheSameTypedValues(XamlInflator inflator, DevicePlatform platform, double expected)
		{
			mockDeviceInfo.Platform = platform;
			var page = new Bugzilla39636(inflator);
			Assert.Equal(expected, page.testLabel.WidthRequest);
			Assert.Equal(expected, page.testBox.WidthRequest);
			Assert.Equal(expected, page.testBox.HeightRequest);
		}

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
