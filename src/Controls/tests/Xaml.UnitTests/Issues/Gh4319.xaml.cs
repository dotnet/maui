using System;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Devices;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Gh4319 : ContentPage
{
	public Gh4319() => InitializeComponent();


	public class Tests : IDisposable
	{
		MockDeviceInfo mockDeviceInfo;

		public Tests()
		{
			DeviceInfo.SetCurrent(mockDeviceInfo = new MockDeviceInfo());
		}

		public void Dispose()
		{
			DeviceInfo.SetCurrent(null);
		}

		[Theory]
		[Values]
		public void OnPlatformMarkupAndNamedSizes(XamlInflator inflator)
		{
			mockDeviceInfo.Platform = DevicePlatform.iOS;
			var layout = new Gh4319(inflator);
			Assert.Equal(4d, layout.label.FontSize);

			mockDeviceInfo.Platform = DevicePlatform.Android;
			layout = new Gh4319(inflator);
			Assert.Equal(8d, layout.label.FontSize);
		}
	}
}