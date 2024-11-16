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
using NUnit.Framework;

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

	}

	[TestFixture]
	class Test
	{
		MockDeviceInfo mockDeviceInfo;

		[SetUp]
		public void Setup()
		{
			Application.SetCurrentApplication(new MockApplication());
			DeviceInfo.SetCurrent(mockDeviceInfo = new MockDeviceInfo());
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		}


		[TearDown]
		public void TearDown()
		{
			AppInfo.SetCurrent(null);
			DeviceInfo.SetCurrent(null);
		}

		[Test]
		public void OnPlatformDownNotThrow([Values(false, true)] bool useCompiledXaml)
		{
			mockDeviceInfo.Platform = DevicePlatform.WinUI;
			Assert.DoesNotThrow(() => new Maui24900(useCompiledXaml));



		}
	}
}