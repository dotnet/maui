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

public partial class Maui20244 : ContentPage
{
	public Maui20244()
	{
		InitializeComponent();
	}

	public Maui20244(bool useCompiledXaml)
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
		public void RowDefStaticResource([Values(false, true)] bool useCompiledXaml)
		{
			if (useCompiledXaml)
				MockCompiler.Compile(typeof(Maui20244));

			var page = new Maui20244(useCompiledXaml);
			var grid = page.grid;

			Assert.That(grid.RowDefinitions.Count, Is.EqualTo(6));
			Assert.That(grid.RowDefinitions[0].Height, Is.EqualTo(new GridLength(1, GridUnitType.Star)));
			Assert.That(grid.RowDefinitions[1].Height, Is.EqualTo(new GridLength(1, GridUnitType.Star)));
			Assert.That(grid.RowDefinitions[2].Height, Is.EqualTo(new GridLength(1, GridUnitType.Star)));
			Assert.That(grid.RowDefinitions[3].Height, Is.EqualTo(new GridLength(1, GridUnitType.Star)));
			Assert.That(grid.RowDefinitions[4].Height, Is.EqualTo(new GridLength(1, GridUnitType.Star)));
			Assert.That(grid.RowDefinitions[5].Height, Is.EqualTo(new GridLength(1, GridUnitType.Auto)));

			Assert.That(grid.ColumnDefinitions.Count, Is.EqualTo(3));
		}
	}

}