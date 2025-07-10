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
	public partial class Maui18980 : ContentPage
	{
		public Maui18980()
		{
			InitializeComponent();
		}
		public Maui18980(bool useCompiledXaml)
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
			public void CSSnotOverridenbyImplicitStyle([Values(false, true)] bool useCompiledXaml)
			{
				// var app = new MockApplication();
				// app.Resources.Add(new Maui18980Style(useCompiledXaml));
				// Application.SetCurrentApplication(app);

				var page = new Maui18980(useCompiledXaml);
				Assert.That(page.button.BackgroundColor, Is.EqualTo(Colors.Red));
			}
		}
	}
}
