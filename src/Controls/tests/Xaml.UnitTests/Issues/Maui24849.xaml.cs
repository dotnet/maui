using System;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Dispatching;

using Microsoft.Maui.Graphics;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui24849 : ContentPage
{
	public Maui24849() => InitializeComponent();

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
		public void VSGReturnsToNormal(XamlInflator inflator)
		{
			var app = new MockApplication();
			app.Resources.Add(new Style24849());
			var page = new Maui24849(inflator);

			app.MainPage = page;

			Assert.False(page.button.IsEnabled);
			Assert.Equal(Color.FromArgb("#3c3c3b"), page.button.TextColor); // TODO: Was FromHex with 2 params, added actual value

			page.button.IsEnabled = true;
			Assert.True(page.button.IsEnabled);
			Assert.Equal(Colors.White, page.button.TextColor);

			page.button.IsEnabled = false;
			Assert.False(page.button.IsEnabled);
			Assert.Equal(Color.FromArgb("#3c3c3b"), page.button.TextColor); // TODO: Was FromHex with 2 params, added actual value
		}
	}
}
