using System;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Devices;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Bz55343 : ContentPage
{
	public Bz55343() => InitializeComponent();


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

		[Theory(Skip = "[Bug] Types that require conversion don't work in OnPlatform: https://github.com/xamarin/Microsoft.Maui.Controls/issues/13830")]
		[Values]
		public void OnPlatformFontConversion(XamlInflator inflator)
		{
			mockDeviceInfo.Platform = DevicePlatform.iOS;
			var layout = new Bz55343(inflator);
			Assert.Equal(16d, layout.label0.FontSize);
			Assert.Equal(64d, layout.label1.FontSize);
		}
	}
}