using System.Linq;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui32697 : ContentPage
{
	public Maui32697() => InitializeComponent();

	[Collection("Issue")]
	public class Tests
	{
		[Theory]
		[XamlInflatorData]
		internal void SetterWithNaNValueWorksWithAllInflators(XamlInflator inflator)
		{
			var page = new Maui32697(inflator);

			// Verify the style was applied
			Assert.NotNull(page.TestLabel);
			Assert.NotNull(page.TestLabel.Style);

			// Verify NaN values are correctly applied
			Assert.True(double.IsNaN(page.TestLabel.HeightRequest), "HeightRequest should be NaN");
			Assert.True(double.IsNaN(page.TestLabel.WidthRequest), "WidthRequest should be NaN");
		}

		[Theory]
		[XamlInflatorData]
		internal void SetterWithNaNValueInStyleResourceWorksWithAllInflators(XamlInflator inflator)
		{
			var page = new Maui32697(inflator);

			// Verify the style exists in resources
			Assert.True(page.Resources.ContainsKey("TestStyle"));

			var style = page.Resources["TestStyle"] as Style;
			Assert.NotNull(style);
			Assert.Equal(2, style.Setters.Count);

			// Verify the setters have NaN values
			var heightSetter = style.Setters.FirstOrDefault(s => s.Property == VisualElement.HeightRequestProperty);
			Assert.NotNull(heightSetter);
			Assert.IsType<double>(heightSetter.Value);
			Assert.True(double.IsNaN((double)heightSetter.Value), "HeightRequest setter value should be NaN");

			var widthSetter = style.Setters.FirstOrDefault(s => s.Property == VisualElement.WidthRequestProperty);
			Assert.NotNull(widthSetter);
			Assert.IsType<double>(widthSetter.Value);
			Assert.True(double.IsNaN((double)widthSetter.Value), "WidthRequest setter value should be NaN");
		}
	}
}
