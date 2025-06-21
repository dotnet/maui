using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Devices;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class Gh4319 : ContentPage
	{
		public Gh4319() => InitializeComponent();
		public Gh4319(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		// [TestFixture] - removed for xUnit
		class Tests
		{
			MockDeviceInfo mockDeviceInfo;

			public void Setup()
			{
				DeviceInfo.SetCurrent(mockDeviceInfo = new MockDeviceInfo());
			}

			public void TearDown()
			{
				DeviceInfo.SetCurrent(null);
			}

			[InlineData(true)], TestCase(false)]
			public void OnPlatformMarkupAndNamedSizes(bool useCompiledXaml)
			{
				mockDeviceInfo.Platform = DevicePlatform.iOS;
				var layout = new Gh4319(useCompiledXaml);
				Assert.Equal(4d, layout.label.FontSize);

				mockDeviceInfo.Platform = DevicePlatform.Android;
				layout = new Gh4319(useCompiledXaml);
				Assert.Equal(8d, layout.label.FontSize);
			}
		}
	}
}