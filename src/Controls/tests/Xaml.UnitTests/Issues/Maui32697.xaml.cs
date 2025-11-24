using System.Linq;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui32697 : ContentPage
{
	public Maui32697() => InitializeComponent();

	[TestFixture]
	class Tests
	{
		[Test]
		public void SetterWithNaNValueWorksWithAllInflators([Values] XamlInflator inflator)
		{
			var page = new Maui32697(inflator);

			// Verify the style was applied
			Assert.That(page.TestLabel, Is.Not.Null);
			Assert.That(page.TestLabel.Style, Is.Not.Null);

			// Verify NaN values are correctly applied
			Assert.That(double.IsNaN(page.TestLabel.HeightRequest), Is.True, "HeightRequest should be NaN");
			Assert.That(double.IsNaN(page.TestLabel.WidthRequest), Is.True, "WidthRequest should be NaN");
		}

		[Test]
		public void SetterWithNaNValueInStyleResourceWorksWithAllInflators([Values] XamlInflator inflator)
		{
			var page = new Maui32697(inflator);

			// Verify the style exists in resources
			Assert.That(page.Resources.ContainsKey("TestStyle"), Is.True);

			var style = page.Resources["TestStyle"] as Style;
			Assert.That(style, Is.Not.Null);
			Assert.That(style.Setters.Count, Is.EqualTo(2));

			// Verify the setters have NaN values
			var heightSetter = style.Setters.FirstOrDefault(s => s.Property == VisualElement.HeightRequestProperty);
			Assert.That(heightSetter, Is.Not.Null);
			Assert.That(heightSetter.Value, Is.TypeOf<double>());
			Assert.That(double.IsNaN((double)heightSetter.Value), Is.True, "HeightRequest setter value should be NaN");

			var widthSetter = style.Setters.FirstOrDefault(s => s.Property == VisualElement.WidthRequestProperty);
			Assert.That(widthSetter, Is.Not.Null);
			Assert.That(widthSetter.Value, Is.TypeOf<double>());
			Assert.That(double.IsNaN((double)widthSetter.Value), Is.True, "WidthRequest setter value should be NaN");
		}
	}
}
