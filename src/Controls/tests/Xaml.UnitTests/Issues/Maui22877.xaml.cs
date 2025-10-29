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

public partial class Maui22877 : ContentPage
{
	public Maui22877()
	{
		InitializeComponent();
	}

	public Maui22877(bool useCompiledXaml)
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
			mockDeviceInfo.Idiom = DeviceIdiom.Phone;
			var page = new Maui22877(useCompiledXaml) { BindingContext = new { BoundString = "BoundString" } };
			Assert.Equal("Grade", page.label0.Text);

			mockDeviceInfo.Idiom = DeviceIdiom.Desktop;
			page = new Maui22877(useCompiledXaml) { BindingContext = new { BoundString = "BoundString" } };
			Assert.Equal("BoundString", page.label0.Text);


		}
	}
}