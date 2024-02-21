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

		[TestFixture]
		class Test
		{
			[SetUp]
			public void Setup()
			{
				Application.SetCurrentApplication(new MockApplication());
				DispatcherProvider.SetCurrent(new DispatcherProviderStub());
			}


			[TearDown] public void TearDown() => AppInfo.SetCurrent(null);

			[Test]
			public void DataTriggerRestoreValue([Values(false, true)] bool useCompiledXaml)
			{
				var page = new Maui18976(useCompiledXaml);
				Assert.That(page.checkbox.IsChecked, Is.False);
				Assert.That(page.button.IsEnabled, Is.True);

				page.checkbox.IsChecked = true;
				Assert.That(page.checkbox.IsChecked, Is.True);
				Assert.That(page.button.IsEnabled, Is.False);

				page.checkbox.IsChecked = false;
				Assert.That(page.checkbox.IsChecked, Is.False);
				Assert.That(page.button.IsEnabled, Is.True);
			}
		}
	}
}