using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Dispatching;

using Microsoft.Maui.Graphics;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui19388 : ContentPage
{
	public Maui19388()
	{
		InitializeComponent();
	}

	public Maui19388(bool useCompiledXaml)
	{
		//this stub will be replaced at compile time
	}	class Test
	{
		MockDeviceInfo mockDeviceInfo;

		// NOTE: xUnit uses constructor for setup. This may need manual conversion.
		// [SetUp]
		public void Setup()
		{
			Application.SetCurrentApplication(new MockApplication());
			DeviceInfo.SetCurrent(mockDeviceInfo = new MockDeviceInfo());
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		}


		// NOTE: xUnit uses IDisposable.Dispose() for teardown. This may need manual conversion.
		// [TearDown]
		public void TearDown()
		{
			AppInfo.SetCurrent(null);
			DeviceInfo.SetCurrent(null);
		}

		[Theory]
			public void Method(bool useCompiledXaml)
		{
			Application.Current.UserAppTheme = AppTheme.Light;
			mockDeviceInfo.Platform = DevicePlatform.iOS;
			var page = new Maui19388(useCompiledXaml);
			Assert.Equal(Colors.Green, page.label0.BackgroundColor);

			mockDeviceInfo.Platform = DevicePlatform.Android;
			page = new Maui19388(useCompiledXaml);
			Assert.Equal(Colors.Red, page.label0.BackgroundColor);


		}
	}
}
