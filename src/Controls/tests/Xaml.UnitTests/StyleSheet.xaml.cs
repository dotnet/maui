using Microsoft.Maui.Graphics;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class StyleSheet : ContentPage
{
	public StyleSheet() => InitializeComponent();

	class Tests
	{
		[Test]
		public void EmbeddedStyleSheetsAreLoaded([Values] XamlInflator inflator)
		{
			var layout = new StyleSheet(inflator);
			Assert.That(layout.Resources.StyleSheets[0].Styles.Count, Is.GreaterThanOrEqualTo(1));
		}

		[Test]
		public void StyleSheetsAreApplied([Values] XamlInflator inflator)
		{
			var layout = new StyleSheet(inflator);
			Assert.That(layout.label0.TextColor, Is.EqualTo(Colors.Azure));
			Assert.That(layout.label0.BackgroundColor, Is.EqualTo(Colors.AliceBlue));
		}
	}
}