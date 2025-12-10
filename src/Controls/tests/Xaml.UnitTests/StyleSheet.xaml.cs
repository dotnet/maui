using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class StyleSheet : ContentPage
{
	public StyleSheet() => InitializeComponent();

	[Collection("Xaml Inflation")]
	public class Tests
	{
		[Theory]
		[XamlInflatorData]
		internal void EmbeddedStyleSheetsAreLoaded(XamlInflator inflator)
		{
			var layout = new StyleSheet(inflator);
			Assert.True(layout.Resources.StyleSheets[0].Styles.Count >= 1);
		}

		[Theory]
		[XamlInflatorData]
		internal void StyleSheetsAreApplied(XamlInflator inflator)
		{
			var layout = new StyleSheet(inflator);
			Assert.Equal(Colors.Azure, layout.label0.TextColor);
			Assert.Equal(Colors.AliceBlue, layout.label0.BackgroundColor);
		}
	}
}