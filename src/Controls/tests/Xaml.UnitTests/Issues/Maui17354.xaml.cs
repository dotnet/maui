using System;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui17354 : ContentPage
{

	public Maui17354() => InitializeComponent();

	public class Test : IDisposable
	{
		public Test()
		{
			AppInfo.SetCurrent(new MockAppInfo());
		}

		public void Dispose()
		{
			AppInfo.SetCurrent(null);
		}

		[Theory]
		[Values]
		public void VSMandAppTheme(XamlInflator inflator)
		{
			var page = new Maui17354(inflator);
			var grid = page.grid;

			Assert.Equal(Colors.Transparent, grid.BackgroundColor);

			Assert.True(VisualStateManager.GoToState(grid, "PointerOver"));
			Assert.Equal(Colors.White, grid.BackgroundColor);

			Assert.True(VisualStateManager.GoToState(grid, "Normal"));
			Assert.Equal(Colors.Transparent, grid.BackgroundColor);

		}
	}
}