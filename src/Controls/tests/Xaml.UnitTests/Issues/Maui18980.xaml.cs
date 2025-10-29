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

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class Maui18980 : ContentPage
	{
		public Maui18980()
		{
			InitializeComponent();
		}
		public Maui18980(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}		class Test
		{
			// NOTE: xUnit uses constructor for setup. This may need manual conversion.
		// [SetUp]
			public void Setup()
			{
				Application.SetCurrentApplication(new MockApplication());
				DispatcherProvider.SetCurrent(new DispatcherProviderStub());
			}


			// NOTE: xUnit uses IDisposable.Dispose() for teardown. This may need manual conversion.
		// [TearDown] public void TearDown() => AppInfo.SetCurrent(null);

			[Theory]
			public void Method(bool useCompiledXaml)
			{
				// var app = new MockApplication();
				// app.Resources.Add(new Maui18980Style(useCompiledXaml));
				// Application.SetCurrentApplication(app);

				var page = new Maui18980(useCompiledXaml);
				Assert.Equal(Colors.Red, page.button.BackgroundColor);
			}
		}
	}
}
