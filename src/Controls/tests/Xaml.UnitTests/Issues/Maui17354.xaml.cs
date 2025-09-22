using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Graphics;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui17354 : ContentPage
{

	public Maui17354() => InitializeComponent();

	class Test
	{
		[SetUp] public void Setup() => AppInfo.SetCurrent(new MockAppInfo());
		[TearDown] public void TearDown() => AppInfo.SetCurrent(null);

		[Test]
		public void VSMandAppTheme([Values] XamlInflator inflator)
		{
			var page = new Maui17354(inflator);
			var grid = page.grid;

			Assert.That(grid.BackgroundColor, Is.EqualTo(Colors.Transparent));

			Assert.True(VisualStateManager.GoToState(grid, "PointerOver"));
			Assert.That(grid.BackgroundColor, Is.EqualTo(Colors.White));

			Assert.True(VisualStateManager.GoToState(grid, "Normal"));
			Assert.That(grid.BackgroundColor, Is.EqualTo(Colors.Transparent));


		}
	}
}