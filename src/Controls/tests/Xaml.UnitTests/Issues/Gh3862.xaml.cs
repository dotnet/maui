using System;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Gh3862 : ContentPage
{
	public Gh3862() => InitializeComponent();


	public class Tests : IDisposable
	{

		public void Dispose() { }
		MockDeviceInfo mockDeviceInfo;

		// TODO: Convert to IDisposable or constructor - [MemberData(nameof(InitializeTest))] // TODO: Convert to IDisposable or constructor public void Setup() => DeviceInfo.SetCurrent(mockDeviceInfo = new MockDeviceInfo());

		[Theory]
		[Values]
		public void OnPlatformMarkupInStyle(XamlInflator inflator)
		{
			mockDeviceInfo.Platform = DevicePlatform.iOS;
			var layout = new Gh3862(inflator);
			Assert.Equal(Colors.Pink, layout.label.TextColor);
			Assert.False(layout.label.IsVisible);

			mockDeviceInfo.Platform = DevicePlatform.Android;

			layout = new Gh3862(inflator);
			Assert.True(layout.label.IsVisible);
		}
	}
}
