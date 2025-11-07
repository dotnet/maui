using System;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Devices;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class OnPlatform : ContentPage
{
	public OnPlatform() => InitializeComponent();


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
		public void BoolToVisibility(XamlInflator inflator)
		{
			mockDeviceInfo.Platform = DevicePlatform.iOS;
			var layout = new OnPlatform(inflator);
			Assert.True(layout.label0.IsVisible);

			mockDeviceInfo.Platform = DevicePlatform.Android;
			layout = new OnPlatform(inflator);
			Assert.False(layout.label0.IsVisible);
		}

		[Theory]
		[Values]
		public void DoubleToWidth(XamlInflator inflator)
		{
			mockDeviceInfo.Platform = DevicePlatform.iOS;
			var layout = new OnPlatform(inflator);
			Assert.Equal(20, layout.label0.WidthRequest);

			mockDeviceInfo.Platform = DevicePlatform.Android;
			layout = new OnPlatform(inflator);
			Assert.Equal(30, layout.label0.WidthRequest);

			mockDeviceInfo.Platform = DevicePlatform.MacCatalyst;
			layout = new OnPlatform(inflator);
			Assert.Equal(0.0, layout.label0.WidthRequest);
		}

		[Theory]
		[Values]
		public void StringToText(XamlInflator inflator)
		{
			mockDeviceInfo.Platform = DevicePlatform.iOS;
			var layout = new OnPlatform(inflator);
			Assert.Equal("Foo", layout.label0.Text);

			mockDeviceInfo.Platform = DevicePlatform.Android;
			layout = new OnPlatform(inflator);
			Assert.Equal("Bar", layout.label0.Text);

			mockDeviceInfo.Platform = DevicePlatform.MacCatalyst;
			layout = new OnPlatform(inflator);
			Assert.Null(layout.label0.Text);
		}

		[Theory]
		[Values]
		public void OnPlatformAsResource(XamlInflator inflator)
		{
			var layout = new OnPlatform(inflator);
			var onplat = layout.Resources["fontAttributes"] as OnPlatform<FontAttributes>;
			Assert.NotNull(onplat);
			mockDeviceInfo.Platform = DevicePlatform.iOS;
			Assert.Equal(FontAttributes.Bold, (FontAttributes)onplat);
			mockDeviceInfo.Platform = DevicePlatform.Android;
			Assert.Equal(FontAttributes.Italic, (FontAttributes)onplat);
			mockDeviceInfo.Platform = DevicePlatform.MacCatalyst;
			Assert.Equal(FontAttributes.None, (FontAttributes)onplat);
		}

		[Theory]
		[Values]
		public void OnPlatformAsResourceAreApplied(XamlInflator inflator)
		{
			mockDeviceInfo.Platform = DevicePlatform.iOS;
			var layout = new OnPlatform(inflator);
			var onidiom = layout.Resources["fontSize"] as OnIdiom<double>;
			Assert.NotNull(onidiom);
			Assert.IsType<double>(onidiom.Phone);
			Assert.Equal(20, onidiom.Phone);
			Assert.Equal(FontAttributes.Bold, layout.label0.FontAttributes);

			mockDeviceInfo.Platform = DevicePlatform.Android;
			layout = new OnPlatform(inflator);
			Assert.Equal(FontAttributes.Italic, layout.label0.FontAttributes);
		}

		[Theory]
		[Values]
		public void OnPlatform2Syntax(XamlInflator inflator)
		{
			mockDeviceInfo.Platform = DevicePlatform.Android;
			var layout = new OnPlatform(inflator);
			Assert.Equal(42, layout.label0.HeightRequest);

			mockDeviceInfo.Platform = DevicePlatform.iOS;
			layout = new OnPlatform(inflator);
			Assert.Equal(21, layout.label0.HeightRequest);

			mockDeviceInfo.Platform = DevicePlatform.MacCatalyst;
			layout = new OnPlatform(inflator);
			Assert.Equal(63.0, layout.label0.HeightRequest);

			mockDeviceInfo.Platform = DevicePlatform.Create("FooBar");
			layout = new OnPlatform(inflator);
			Assert.Equal(42, layout.label0.HeightRequest);
		}

		[Theory]
		[Values]
		public void OnPlatformDefault(XamlInflator inflator)
		{
			mockDeviceInfo.Platform = DevicePlatform.Create("\ud83d\ude80");
			var layout = new OnPlatform(inflator);
			Assert.Equal(63, layout.label0.HeightRequest);
		}

		[Theory]
		[Values]
		public void OnPlatformInStyle0(XamlInflator inflator)
		{
			mockDeviceInfo.Platform = DevicePlatform.iOS;
			var layout = new OnPlatform(inflator);
			Assert.Equal(36, layout.button0.FontSize);

			mockDeviceInfo.Platform = DevicePlatform.MacCatalyst;
			layout = new OnPlatform(inflator);
			Assert.Equal(0.0, layout.button0.FontSize);
		}

		[Theory]
		[Values]
		public void OnPlatformInStyle1(XamlInflator inflator)
		{
			mockDeviceInfo.Platform = DevicePlatform.iOS;
			var layout = new OnPlatform(inflator);
			Assert.Equal(36, layout.button1.FontSize);

			mockDeviceInfo.Platform = DevicePlatform.MacCatalyst;
			layout = new OnPlatform(inflator);
			Assert.Equal(0.0, layout.button1.FontSize);
		}

		[Theory]
		[Values]
		public void OnPlatformInline(XamlInflator inflator)
		{
			mockDeviceInfo.Platform = DevicePlatform.iOS;
			var layout = new OnPlatform(inflator);
			Assert.Equal(36, layout.button2.FontSize);

			mockDeviceInfo.Platform = DevicePlatform.MacCatalyst;
			layout = new OnPlatform(inflator);
			Assert.Equal(0.0, layout.button2.FontSize);
		}
	}
}