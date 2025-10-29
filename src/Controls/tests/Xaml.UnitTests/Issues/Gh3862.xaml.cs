using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class Gh3862 : ContentPage
	{
		public Gh3862()
		{
			InitializeComponent();
		}

		public Gh3862(bool useCompiledXaml)
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

			[InlineData(false), InlineData(true)]
			public void OnPlatformMarkupInStyle(bool useCompiledXaml)
			{
				mockDeviceInfo.Platform = DevicePlatform.iOS;
				var layout = new Gh3862(useCompiledXaml);
				Assert.Equal(Colors.Pink, layout.label.TextColor);
				Assert.True(layout.label.IsVisible, Is.False);

				mockDeviceInfo.Platform = DevicePlatform.Android;

				layout = new Gh3862(useCompiledXaml);
				Assert.True(layout.label.IsVisible, Is.True);

			}
		}
	}
}
