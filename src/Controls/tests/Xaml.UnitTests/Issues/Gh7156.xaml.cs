using System;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Devices;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Gh7156 : ContentPage
{
	public Gh7156() => InitializeComponent();


	public class Tests : IDisposable
	{

		public void Dispose() { }
		MockDeviceInfo mockDeviceInfo;

		// TODO: Convert to IDisposable or constructor - [MemberData(nameof(InitializeTest))] // TODO: Convert to IDisposable or constructor public void Setup() => DeviceInfo.SetCurrent(mockDeviceInfo = new MockDeviceInfo());

		[Theory]
		[Values]
		public void OnPlatformDefaultToBPDefaultValue(XamlInflator inflator)
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
