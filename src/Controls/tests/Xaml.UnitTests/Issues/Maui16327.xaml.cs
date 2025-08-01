using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Devices;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui16327 : ContentPage
{
	public Maui16327() => InitializeComponent();

	class Test
	{
		MockDeviceInfo mockDeviceInfo;

		[SetUp]
		public void Setup()
		{
			AppInfo.SetCurrent(new MockAppInfo());
			DeviceInfo.SetCurrent(mockDeviceInfo = new MockDeviceInfo());

		}

		[TearDown]
		public void TearDown()
		{
			DeviceInfo.SetCurrent(mockDeviceInfo = null);
			AppInfo.SetCurrent(null);
		}

		[Test]
		public void ConversionOfResources([Values] XamlInflator inflator)
		{
			mockDeviceInfo.Platform = DevicePlatform.iOS;

			var page = new Maui16327(inflator);
			var border = page.border;

			var shape = border.StrokeShape as RoundRectangle;
			Assert.That(shape.CornerRadius.BottomLeft, Is.EqualTo(10));
		}
	}
}