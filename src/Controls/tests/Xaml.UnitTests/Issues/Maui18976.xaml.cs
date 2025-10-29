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
	public partial class Maui18976 : ContentPage
	{
		public Maui18976()
		{
			InitializeComponent();
		}

		public Maui18976(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}
		public class Test
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
			[InlineData(false)]
			[InlineData(true)]
			public void Method(bool useCompiledXaml)
			{
				var page = new Maui18976(useCompiledXaml);
				Assert.True(page.checkbox.IsChecked, Is.False);
				Assert.True(page.button.IsEnabled, Is.True);

				page.checkbox.IsChecked = true;
				Assert.True(page.checkbox.IsChecked, Is.True);
				Assert.True(page.button.IsEnabled, Is.False);

				page.checkbox.IsChecked = false;
				Assert.True(page.checkbox.IsChecked, Is.False);
				Assert.True(page.button.IsEnabled, Is.True);
			}
		}
	}
}