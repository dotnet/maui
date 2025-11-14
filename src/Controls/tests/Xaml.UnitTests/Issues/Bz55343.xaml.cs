using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Devices;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Bz55343 : ContentPage
{
	public Bz55343() => InitializeComponent();

	[TestFixture]
	class Tests
	{
		MockDeviceInfo mockDeviceInfo;

		[SetUp] public void Setup() => DeviceInfo.SetCurrent(mockDeviceInfo = new MockDeviceInfo());

		[TearDown] public void TearDown() => DeviceInfo.SetCurrent(null);

		[Ignore("[Bug] Types that require conversion don't work in OnPlatform: https://github.com/xamarin/Microsoft.Maui.Controls/issues/13830")]
		[Test]
		public void OnPlatformFontConversion([Values] XamlInflator inflator)
		{
			mockDeviceInfo.Platform = DevicePlatform.iOS;
			var layout = new Bz55343(inflator);
			Assert.That(layout.label0.FontSize, Is.EqualTo(16d));
			Assert.That(layout.label1.FontSize, Is.EqualTo(64d));
		}
	}
}