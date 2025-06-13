using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui16327 : ContentPage
{

	public Maui16327() => InitializeComponent();

	public Maui16327(bool useCompiledXaml)
	{
		//this stub will be replaced at compile time
	}

	// [TestFixture] - removed for xUnit
	class Test
	{
		MockDeviceInfo mockDeviceInfo;

		public void Setup()
		{
			AppInfo.SetCurrent(new MockAppInfo());
			DeviceInfo.SetCurrent(mockDeviceInfo = new MockDeviceInfo());

		}

		public void TearDown()
		{
			DeviceInfo.SetCurrent(mockDeviceInfo = null);
			AppInfo.SetCurrent(null);
		}

		[Fact]
		public void ConversionOfResources([Values(false, true)] bool useCompiledXaml)
		{
			mockDeviceInfo.Platform = DevicePlatform.iOS;

			var page = new Maui16327(useCompiledXaml);
			var border = page.border;

			var shape = border.StrokeShape as RoundRectangle;
			Assert.Equal(10, shape.CornerRadius.BottomLeft);
		}
	}
}