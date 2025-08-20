using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Devices;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Gh4319 : ContentPage
{
	public Gh4319() => InitializeComponent();

	[TestFixture]
	class Tests
	{
		MockDeviceInfo mockDeviceInfo;

		[SetUp] public void Setup() => DeviceInfo.SetCurrent(mockDeviceInfo = new MockDeviceInfo());

		[TearDown] public void TearDown() => DeviceInfo.SetCurrent(null);

		[Test]
#if FIXME_BEFORE_PUBLIC_RELEASE
		public void OnPlatformMarkupAndNamedSizes([Values(XamlInflator.XamlC, XamlInflator.Runtime)] XamlInflator inflator)
#else
		public void OnPlatformMarkupAndNamedSizes([Values] XamlInflator inflator)
#endif
		{
			mockDeviceInfo.Platform = DevicePlatform.iOS;
			var layout = new Gh4319(inflator);
			Assert.That(layout.label.FontSize, Is.EqualTo(4d));

			mockDeviceInfo.Platform = DevicePlatform.Android;
			layout = new Gh4319(inflator);
			Assert.That(layout.label.FontSize, Is.EqualTo(8d));
		}
	}
}