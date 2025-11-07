using System;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui24500 : ContentPage
{
	public Maui24500() => InitializeComponent();

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
		public void OnIdiomBindingValueTypeRelease(XamlInflator inflator)
		{
			mockDeviceInfo.Idiom = DeviceIdiom.Phone;
			var page = new Maui24500(inflator) { BindingContext = new { EditingMode = true } };
			Assert.False(page.label0.IsVisible);

			mockDeviceInfo.Idiom = DeviceIdiom.Desktop;
			page = new Maui24500(inflator) { BindingContext = new { EditingMode = true } };
			Assert.True(page.label0.IsVisible);

		}
	}
}
