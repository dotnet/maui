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
	}	class Test
	{
		MockDeviceInfo mockDeviceInfo;

		// NOTE: xUnit uses constructor for setup. This may need manual conversion.
		// [SetUp]
		public void Setup()
		{
			AppInfo.SetCurrent(new MockAppInfo());
			DeviceInfo.SetCurrent(mockDeviceInfo = new MockDeviceInfo());

		}

		// NOTE: xUnit uses IDisposable.Dispose() for teardown. This may need manual conversion.
		// [TearDown]
		public void TearDown()
		{
			DeviceInfo.SetCurrent(mockDeviceInfo = null);
			AppInfo.SetCurrent(null);
		}

		[Theory]
			public void Method(bool useCompiledXaml)
		{
			mockDeviceInfo.Platform = DevicePlatform.iOS;

			var page = new Maui16327(useCompiledXaml);
			var border = page.border;

			var shape = border.StrokeShape as RoundRectangle;
			Assert.Equal(10, shape.CornerRadius.BottomLeft);
		}
	}
}