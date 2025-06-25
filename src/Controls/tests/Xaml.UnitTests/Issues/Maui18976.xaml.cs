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
		}		class Test
		{
			[SetUp]
			public void Setup()
			{
				Application.SetCurrentApplication(new MockApplication());
				DispatcherProvider.SetCurrent(new DispatcherProviderStub());
			}

			[TearDown] public void TearDown() => AppInfo.SetCurrent(null);

			[Fact]
			public void DataTriggerRestoreValue([Values(false, true)] bool useCompiledXaml)
			{
				var page = new Maui18976(useCompiledXaml);
				Assert.False(page.checkbox.IsChecked);
				Assert.True(page.button.IsEnabled);

				page.checkbox.IsChecked = true;
				Assert.True(page.checkbox.IsChecked);
				Assert.False(page.button.IsEnabled);

				page.checkbox.IsChecked = false;
				Assert.False(page.checkbox.IsChecked);
				Assert.True(page.button.IsEnabled);
			}
		}
	}
}