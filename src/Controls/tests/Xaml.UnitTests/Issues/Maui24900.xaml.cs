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

public partial class Maui24900 : ContentPage
{
	public Maui24900()
	{
		InitializeComponent();
	}

	public Maui24900(bool useCompiledXaml)
	{
		//this stub will be replaced at compile time
	}

	void Header_Tapped(object sender, TappedEventArgs e)
	{

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
			public void Method([InlineData(false, true)] bool useCompiledXaml)
		{
			mockDeviceInfo.Platform = DevicePlatform.WinUI;
			Assert.DoesNotThrow(() => new Maui24900(useCompiledXaml));



		}
	}
}