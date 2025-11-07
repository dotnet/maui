using System;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Devices;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui16327 : ContentPage
{
	public Maui16327() => InitializeComponent();

	public class Test
	{
		MockDeviceInfo mockDeviceInfo;

		public Test()
		{
			AppInfo.SetCurrent(new MockAppInfo());
			DeviceInfo.SetCurrent(mockDeviceInfo = new MockDeviceInfo());

		}

		public void Dispose()
		{
			DeviceInfo.SetCurrent(mockDeviceInfo = null);
			AppInfo.SetCurrent(null);
		}

		[Theory]
		[Values]
		public void ConversionOfResources(XamlInflator inflator)
		{
			mockDeviceInfo.Platform = DevicePlatform.iOS;

			var page = new Maui16327(inflator);
			var border = page.border;

			var shape = border.StrokeShape as RoundRectangle;
			Assert.Equal(10, shape.CornerRadius.BottomLeft);
		}
	}
}
