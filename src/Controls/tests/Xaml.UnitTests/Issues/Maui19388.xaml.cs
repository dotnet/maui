using System;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Dispatching;

using Microsoft.Maui.Graphics;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui19388 : ContentPage
{
	public Maui19388() => InitializeComponent();

	public class Test : IDisposable
	{
		MockDeviceInfo mockDeviceInfo;

		public Test()
		{
			Application.SetCurrentApplication(new MockApplication());
			DeviceInfo.SetCurrent(mockDeviceInfo = new MockDeviceInfo());
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		}

		public void Dispose()
		{
			AppInfo.SetCurrent(null);
			DeviceInfo.SetCurrent(null);
		}

		[Theory]
		[Values]
		public void OnPlatformAppThemeBindingRelease(XamlInflator inflator)
		{
			Application.Current.UserAppTheme = AppTheme.Light;
			mockDeviceInfo.Platform = DevicePlatform.iOS;
			var page = new Maui19388(inflator);
			Assert.Equal(Colors.Green, page.label0.BackgroundColor);

			mockDeviceInfo.Platform = DevicePlatform.Android;
			page = new Maui19388(inflator);
			Assert.Equal(Colors.Red, page.label0.BackgroundColor);
		}
	}
}
