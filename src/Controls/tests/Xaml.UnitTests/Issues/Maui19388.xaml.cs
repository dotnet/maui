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

public partial class Maui19388 : ContentPage
{
	public Maui19388()
	{
		InitializeComponent();
	}

	public Maui19388(bool useCompiledXaml)
	{
		//this stub will be replaced at compile time
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
		public void OnPlatformAppThemeBindingRelease([Values(false, true)] bool useCompiledXaml)
		{
			Application.Current.UserAppTheme = AppTheme.Light;
			mockDeviceInfo.Platform = DevicePlatform.iOS;
			var page = new Maui19388(useCompiledXaml);
			Assert.That(page.label0.BackgroundColor, Is.EqualTo(Colors.Green));

			mockDeviceInfo.Platform = DevicePlatform.Android;
			page = new Maui19388(useCompiledXaml);
			Assert.That(page.label0.BackgroundColor, Is.EqualTo(Colors.Red));


		}
	}
}
