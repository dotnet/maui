using System;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Devices;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Gh7156 : ContentPage
{
	public Gh7156() => InitializeComponent();

	[Collection("Issue")]
	public class Tests : IDisposable
	{
		MockDeviceInfo mockDeviceInfo;

		public Tests() => DeviceInfo.SetCurrent(mockDeviceInfo = new MockDeviceInfo());

		public void Dispose() => DeviceInfo.SetCurrent(null);

		[Theory]
		[XamlInflatorData]
		internal void OnPlatformDefaultToBPDefaultValue(XamlInflator inflator)
		{
			mockDeviceInfo.Platform = DevicePlatform.Android;
			var layout = new Gh7156(inflator);
			Assert.Equal(Label.TextProperty.DefaultValue, layout.l0.Text);
			Assert.Equal(VisualElement.WidthRequestProperty.DefaultValue, layout.l0.WidthRequest);
			Assert.Equal("bar", layout.l1.Text);
			Assert.Equal(20d, layout.l1.WidthRequest);
		}
	}
}
