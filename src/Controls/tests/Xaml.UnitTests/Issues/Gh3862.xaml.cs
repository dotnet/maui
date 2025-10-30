using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Graphics;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Gh3862 : ContentPage
{
	public Gh3862() => InitializeComponent();

	[TestFixture]
	class Tests
	{
		MockDeviceInfo mockDeviceInfo;

		[SetUp] public void Setup() => DeviceInfo.SetCurrent(mockDeviceInfo = new MockDeviceInfo());

		[TearDown] public void TearDown() => DeviceInfo.SetCurrent(null);

		[Test]
		public void OnPlatformMarkupInStyle([Values] XamlInflator inflator)
		{
			mockDeviceInfo.Platform = DevicePlatform.iOS;
			var layout = new Gh3862(inflator);
			Assert.That(layout.label.TextColor, Is.EqualTo(Colors.Pink));
			Assert.That(layout.label.IsVisible, Is.False);

			mockDeviceInfo.Platform = DevicePlatform.Android;

			layout = new Gh3862(inflator);
			Assert.That(layout.label.IsVisible, Is.True);
		}
	}
}
