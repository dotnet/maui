using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Devices;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class OnPlatform : ContentPage
	{
		public OnPlatform()
		{
			InitializeComponent();
		}

		public OnPlatform(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}
		public public class Tests
		{
			MockDeviceInfo mockDeviceInfo;

			// NOTE: xUnit uses constructor for setup. This may need manual conversion.
			// [SetUp]
			[Xunit.Fact]
			public void Setup()
			{
				DeviceInfo.SetCurrent(mockDeviceInfo = new MockDeviceInfo());
			}

			// NOTE: xUnit uses IDisposable.Dispose() for teardown. This may need manual conversion.
			// [TearDown]
			[Xunit.Fact]
			public void TearDown()
			{
				DeviceInfo.SetCurrent(null);
			}

			[InlineData(false)]
			[Theory]
			[InlineData(true)]
			public void BoolToVisibility(bool useCompiledXaml)
			{
				mockDeviceInfo.Platform = DevicePlatform.iOS;
				var layout = new OnPlatform(useCompiledXaml);
				Assert.True(layout.label0.IsVisible);

				mockDeviceInfo.Platform = DevicePlatform.Android;
				layout = new OnPlatform(useCompiledXaml);
				Assert.False(layout.label0.IsVisible);
			}

			[InlineData(false)]
			[Theory]
			[InlineData(true)]
			public void DoubleToWidth(bool useCompiledXaml)
			{
				mockDeviceInfo.Platform = DevicePlatform.iOS;
				var layout = new OnPlatform(useCompiledXaml);
				Assert.Equal(20, layout.label0.WidthRequest);

				mockDeviceInfo.Platform = DevicePlatform.Android;
				layout = new OnPlatform(useCompiledXaml);
				Assert.Equal(30, layout.label0.WidthRequest);

				mockDeviceInfo.Platform = DevicePlatform.MacCatalyst;
				layout = new OnPlatform(useCompiledXaml);
				Assert.Equal(0.0, layout.label0.WidthRequest);
			}

			[InlineData(false)]
			[Theory]
			[InlineData(true)]
			public void StringToText(bool useCompiledXaml)
			{
				mockDeviceInfo.Platform = DevicePlatform.iOS;
				var layout = new OnPlatform(useCompiledXaml);
				Assert.Equal("Foo", layout.label0.Text);

				mockDeviceInfo.Platform = DevicePlatform.Android;
				layout = new OnPlatform(useCompiledXaml);
				Assert.Equal("Bar", layout.label0.Text);

				mockDeviceInfo.Platform = DevicePlatform.MacCatalyst;
				layout = new OnPlatform(useCompiledXaml);
				Assert.Null(layout.label0.Text);
			}

			[InlineData(false)]
			[Theory]
			[InlineData(true)]
			public void OnPlatformAsResource(bool useCompiledXaml)
			{
				var layout = new OnPlatform(useCompiledXaml);
				var onplat = layout.Resources["fontAttributes"] as OnPlatform<FontAttributes>;
				Assert.NotNull(onplat);
				mockDeviceInfo.Platform = DevicePlatform.iOS;
				Assert.Equal(FontAttributes.Bold, (FontAttributes)onplat);
				mockDeviceInfo.Platform = DevicePlatform.Android;
				Assert.Equal(FontAttributes.Italic, (FontAttributes)onplat);
				mockDeviceInfo.Platform = DevicePlatform.MacCatalyst;
				Assert.Equal(FontAttributes.None, (FontAttributes)onplat);
			}

			[InlineData(false)]
			[Theory]
			[InlineData(true)]
			public void OnPlatformAsResourceAreApplied(bool useCompiledXaml)
			{
				mockDeviceInfo.Platform = DevicePlatform.iOS;
				var layout = new OnPlatform(useCompiledXaml);
				var onidiom = layout.Resources["fontSize"] as OnIdiom<double>;
				Assert.NotNull(onidiom);
				Assert.IsType<double>(onidiom.Phone);
				Assert.Equal(20, onidiom.Phone);
				Assert.Equal(FontAttributes.Bold, layout.label0.FontAttributes);

				mockDeviceInfo.Platform = DevicePlatform.Android;
				layout = new OnPlatform(useCompiledXaml);
				Assert.Equal(FontAttributes.Italic, layout.label0.FontAttributes);
			}

			[InlineData(false)]
			[Theory]
			[InlineData(true)]
			public void OnPlatform2Syntax(bool useCompiledXaml)
			{
				mockDeviceInfo.Platform = DevicePlatform.Android;
				var layout = new OnPlatform(useCompiledXaml);
				Assert.Equal(42, layout.label0.HeightRequest);

				mockDeviceInfo.Platform = DevicePlatform.iOS;
				layout = new OnPlatform(useCompiledXaml);
				Assert.Equal(21, layout.label0.HeightRequest);

				mockDeviceInfo.Platform = DevicePlatform.MacCatalyst;
				layout = new OnPlatform(useCompiledXaml);
				Assert.Equal(63.0, layout.label0.HeightRequest);

				mockDeviceInfo.Platform = DevicePlatform.Create("FooBar");
				layout = new OnPlatform(useCompiledXaml);
				Assert.Equal(42, layout.label0.HeightRequest);
			}

			[InlineData(false)]
			[Theory]
			[InlineData(true)]
			public void OnPlatformDefault(bool useCompiledXaml)
			{
				mockDeviceInfo.Platform = DevicePlatform.Create("\ud83d\ude80");
				var layout = new OnPlatform(useCompiledXaml);
				Assert.Equal(63, layout.label0.HeightRequest);
			}

			[InlineData(false)]
			[Theory]
			[InlineData(true)]
			public void OnPlatformInStyle0(bool useCompiledXaml)
			{
				mockDeviceInfo.Platform = DevicePlatform.iOS;
				var layout = new OnPlatform(useCompiledXaml);
				Assert.Equal(36, layout.button0.FontSize);

				mockDeviceInfo.Platform = DevicePlatform.MacCatalyst;
				layout = new OnPlatform(useCompiledXaml);
				Assert.Equal(0.0, layout.button0.FontSize);
			}

			[InlineData(false)]
			[Theory]
			[InlineData(true)]
			public void OnPlatformInStyle1(bool useCompiledXaml)
			{
				mockDeviceInfo.Platform = DevicePlatform.iOS;
				var layout = new OnPlatform(useCompiledXaml);
				Assert.Equal(36, layout.button1.FontSize);

				mockDeviceInfo.Platform = DevicePlatform.MacCatalyst;
				layout = new OnPlatform(useCompiledXaml);
				Assert.Equal(0.0, layout.button1.FontSize);
			}

			[InlineData(false)]
			[Theory]
			[InlineData(true)]
			public void OnPlatformInline(bool useCompiledXaml)
			{
				mockDeviceInfo.Platform = DevicePlatform.iOS;
				var layout = new OnPlatform(useCompiledXaml);
				Assert.Equal(36, layout.button2.FontSize);

				mockDeviceInfo.Platform = DevicePlatform.MacCatalyst;
				layout = new OnPlatform(useCompiledXaml);
				Assert.Equal(0.0, layout.button2.FontSize);
			}
		}
	}
}