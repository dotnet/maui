using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

// Test for: Issue #28854 — Background property works correctly in XAML (BackgroundColor deprecated in favour of Background)
public partial class Maui28854 : ContentPage
{
	public Maui28854() => InitializeComponent();

	[Collection("Issue")]
	public class Tests
	{
		[Theory]
		[XamlInflatorData]
		internal void BackgroundSolidColorBrushSetInXaml(XamlInflator inflator)
		{
			var page = new Maui28854(inflator);

			Assert.NotNull(page.labelSolidBrush);
			var brush = Assert.IsType<SolidColorBrush>(page.labelSolidBrush.Background);
			Assert.Equal(Colors.Red, brush.Color);
		}

		[Theory]
		[XamlInflatorData]
		internal void BackgroundColorStringConvertsToSolidColorBrush(XamlInflator inflator)
		{
			var page = new Maui28854(inflator);

			Assert.NotNull(page.labelColorString);
			var brush = Assert.IsType<SolidColorBrush>(page.labelColorString.Background);
			Assert.Equal(Colors.Blue, brush.Color);
		}

		[Theory]
		[XamlInflatorData]
		internal void BackgroundLinearGradientSetInXaml(XamlInflator inflator)
		{
			var page = new Maui28854(inflator);

			Assert.NotNull(page.boxViewGradient);
			var gradient = Assert.IsType<LinearGradientBrush>(page.boxViewGradient.Background);
			Assert.Equal(2, gradient.GradientStops.Count);
			Assert.Equal(Colors.Red, gradient.GradientStops[0].Color);
			Assert.Equal(Colors.Blue, gradient.GradientStops[1].Color);
		}

		[Theory]
		[XamlInflatorData]
		internal void BackgroundIsNullWhenNotSet(XamlInflator inflator)
		{
			var page = new Maui28854(inflator);

			Assert.NotNull(page.labelNoBackground);
			Assert.True(Brush.IsNullOrEmpty(page.labelNoBackground.Background));
		}

		[Theory]
		[XamlInflatorData]
		internal void IViewBackgroundReturnsBrushWhenBackgroundSet(XamlInflator inflator)
		{
			var page = new Maui28854(inflator);
			IView view = page.labelSolidBrush;

			// IView.Background uses the Brush→Paint implicit operator: SolidColorBrush → SolidPaint
			Assert.NotNull(view.Background);
			var paint = Assert.IsType<SolidPaint>(view.Background);
			Assert.Equal(Colors.Red, paint.Color);
		}

		[Theory]
		[XamlInflatorData]
		internal void IViewBackgroundReturnsNullWhenNeitherSet(XamlInflator inflator)
		{
			var page = new Maui28854(inflator);
			IView view = page.labelNoBackground;

			// IView.Background should return null when no Background or BackgroundColor is set
			Assert.Null(view.Background);
		}
	}
}
