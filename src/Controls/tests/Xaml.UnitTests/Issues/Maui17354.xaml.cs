using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui17354 : ContentPage
{

	public Maui17354() => InitializeComponent();

	public Maui17354(bool useCompiledXaml)
	{
		//this stub will be replaced at compile time
	}
	class Test
	{
		// Constructor public void Setup() => AppInfo.SetCurrent(new MockAppInfo());
		// IDisposable public void TearDown() => AppInfo.SetCurrent(null);

		[Theory]
		public void VSMandAppTheme([Theory]
		[InlineData(false)]
		[InlineData(true)] bool useCompiledXaml)
		{
			var page = new Maui17354(useCompiledXaml);
			var grid = page.grid;

			Assert.Equal(Colors.Transparent, grid.BackgroundColor);

			Assert.True(VisualStateManager.GoToState(grid, "PointerOver"));
			Assert.Equal(Colors.White, grid.BackgroundColor);

			Assert.True(VisualStateManager.GoToState(grid, "Normal"));
			Assert.Equal(Colors.Transparent, grid.BackgroundColor);

		}
	}
}