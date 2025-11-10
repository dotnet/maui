using System;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui16327 : ContentPage
{
	public Maui16327() => InitializeComponent();

	public class Test : IDisposable
	{
		MockDeviceInfo mockDeviceInfo;

		public Test()
		{
			Application.SetCurrentApplication(new MockApplication());
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
			AppInfo.SetCurrent(new MockAppInfo());
			DeviceInfo.SetCurrent(mockDeviceInfo = new MockDeviceInfo());
		}

		public void Dispose()
		{
			DeviceInfo.SetCurrent(null);
			AppInfo.SetCurrent(null);
			DispatcherProvider.SetCurrent(null);
			Application.SetCurrentApplication(null);
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
