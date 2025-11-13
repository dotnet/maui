using System;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui24900 : ContentPage
{
	public Maui24900() => InitializeComponent();

	void Header_Tapped(object sender, TappedEventArgs e)
	{

	}

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
			Application.Current = null;
			DispatcherProvider.SetCurrent(null);
		}

		[Theory]
		[Values]
		public void OnPlatformDoesNotThrow(XamlInflator inflator)
		{
			mockDeviceInfo.Platform = DevicePlatform.WinUI;
			var page = new Maui24900(inflator);
			Assert.NotNull(page);
		}
	}
}
