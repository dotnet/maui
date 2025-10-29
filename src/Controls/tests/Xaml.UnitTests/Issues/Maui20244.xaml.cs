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
			if (useCompiledXaml)
				MockCompiler.Compile(typeof(Maui20244));

			var page = new Maui20244(useCompiledXaml);
			var grid = page.grid;

			Assert.Equal(6, grid.RowDefinitions.Count);
			Assert.Equal(new GridLength(1, GridUnitType.Star, grid.RowDefinitions[0].Height));
			Assert.Equal(new GridLength(1, GridUnitType.Star, grid.RowDefinitions[1].Height));
			Assert.Equal(new GridLength(1, GridUnitType.Star, grid.RowDefinitions[2].Height));
			Assert.Equal(new GridLength(1, GridUnitType.Star, grid.RowDefinitions[3].Height));
			Assert.Equal(new GridLength(1, GridUnitType.Star, grid.RowDefinitions[4].Height));
			Assert.Equal(new GridLength(1, GridUnitType.Auto, grid.RowDefinitions[5].Height));

			Assert.Equal(3, grid.ColumnDefinitions.Count);
		}
	}

}