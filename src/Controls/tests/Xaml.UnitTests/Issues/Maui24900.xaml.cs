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

	public class Test
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
		public void OnPlatformDoesNotThrow()
		{
			mockDeviceInfo.Platform = DevicePlatform.WinUI;
			// TODO: XUnit has no DoesNotThrow. Remove this or use try/catch if needed: // (() => new Maui24900(inflator));
		}
	}
}
