using System;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Devices;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Bz44213 : ContentPage
{
	public Bz44213()
	{
		InitializeComponent();
	}


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
		public void BindingInOnPlatform(XamlInflator inflator)
		{
			mockDeviceInfo.Platform = DevicePlatform.iOS;
			var p = new Bz44213(inflator);
			p.BindingContext = new { Foo = "Foo", Bar = "Bar" };
			Assert.Equal("Foo", p.label.Text);
			mockDeviceInfo.Platform = DevicePlatform.Android;
			p = new Bz44213(inflator);
			p.BindingContext = new { Foo = "Foo", Bar = "Bar" };
			Assert.Equal("Bar", p.label.Text);
		}
	}
}
