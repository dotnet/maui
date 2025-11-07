using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class StyleSheet : ContentPage
{
	public StyleSheet() => InitializeComponent();

	public class Tests
	{
		[Theory]
		[Values]
		public void EmbeddedStyleSheetsAreLoaded(XamlInflator inflator)
		{
			var layout = new StyleSheet(inflator);
			Assert.True(layout.Resources.StyleSheets[0].Styles.Count >= 1);
		}

		[Theory]
		[Values]
		public void StyleSheetsAreApplied(XamlInflator inflator)
		{
			var layout = new StyleSheet(inflator);
			Assert.Equal(Colors.Azure, layout.label0.TextColor);
			Assert.Equal(Colors.AliceBlue, layout.label0.BackgroundColor);
		}
	}
}