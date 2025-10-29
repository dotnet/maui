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

public partial class Maui24849 : ContentPage
{
	public Maui24849()
	{
		InitializeComponent();
	}

	public Maui24849(bool useCompiledXaml)
	{
		//this stub will be replaced at compile time
	}
	public class Test
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
		[InlineData(false)]
		[InlineData(true)]
		public void Method(bool useCompiledXaml)
		{
			var app = new MockApplication();
			app.Resources.Add(new Style24849());
			var page = new Maui24849(useCompiledXaml);

			app.MainPage = page;

			Assert.True(page.button.IsEnabled, Is.False);
			Assert.Equal(Color.FromHex("#3c3c3b", page.button.TextColor));

			page.button.IsEnabled = true;
			Assert.True(page.button.IsEnabled, Is.True);
			Assert.Equal(Colors.White, page.button.TextColor);

			page.button.IsEnabled = false;
			Assert.True(page.button.IsEnabled, Is.False);
			Assert.Equal(Color.FromHex("#3c3c3b", page.button.TextColor));
		}
	}
}