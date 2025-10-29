using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Devices;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class Gh7156 : ContentPage
	{
		public Gh7156() => InitializeComponent();
		public Gh7156(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}
		public class Tests
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

			[Theory]
			[InlineData(false)]
			[InlineData(true)]
			public void Method(bool useCompiledXaml)
			{
				mockDeviceInfo.Platform = DevicePlatform.Android;
				var layout = new Gh7156(useCompiledXaml);
				Assert.Equal(Label.TextProperty.DefaultValue, layout.l0.Text);
				Assert.Equal(VisualElement.WidthRequestProperty.DefaultValue, layout.l0.WidthRequest);
				Assert.Equal("bar", layout.l1.Text);
				Assert.Equal(20d, layout.l1.WidthRequest);
			}
		}
	}
}
