using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Devices;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Bugzilla39636 : ContentPage
{
	public Bugzilla39636() => InitializeComponent();

	[TestFixture]
	class Tests
	{
		MockDeviceInfo mockDeviceInfo;

		[SetUp]
		public void Setup()
		{
			Application.SetCurrentApplication(new MockApplication());
			DeviceInfo.SetCurrent(mockDeviceInfo = new MockDeviceInfo());
		}

		[TearDown] public void TearDown() => DeviceInfo.SetCurrent(null);

		[Test]
		public void OnPlatformWithMissingTargetPlatformShouldUseDefault([Values] XamlInflator inflator)
		{
			// Reproduces Bugzilla39636: When MacCatalyst is not defined in OnPlatform,
			// all inflators should use default(T) instead of throwing an exception

			// Test with MacCatalyst where platform is not defined
			mockDeviceInfo.Platform = DevicePlatform.MacCatalyst;
			var page = new Bugzilla39636(inflator);

			// Should use default value (0.0 for double) instead of throwing
			Assert.That(page.testLabel, Is.Not.Null);
			Assert.That(page.testLabel.WidthRequest, Is.EqualTo(0.0));
		}
	}
}

