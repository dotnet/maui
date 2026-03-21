using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class FontImageExtension : TabBar
{
	public FontImageExtension() => InitializeComponent();

	public static string FontFamily => "MyFontFamily";
	public static string Glyph => "MyGlyph";
	public static Color Color => Colors.Black;
	public static double Size = 50d;

	[Collection("Xaml Inflation")]
	public class Tests
	{
		[Theory]
		[XamlInflatorData]
		internal void FontImageExtension_Positive(XamlInflator inflator)
		{
			var layout = new FontImageExtension(inflator);
			var tabs = ((IVisualTreeElement)layout).GetVisualChildren();

			int i = 0;
			foreach (var tab in tabs)
			{
				Tab myTab = (Tab)tab;
				if (myTab == null)
					continue;

				Assert.IsType<FontImageSource>(myTab.Icon);

				var fontImage = (FontImageSource)myTab.Icon;
				Assert.Equal(FontFamily, fontImage.FontFamily);
				Assert.Equal(Glyph, fontImage.Glyph);

				if (i == 3)
					Assert.Equal(30d, fontImage.Size);
				else
					Assert.Equal(Size, fontImage.Size);

				Assert.Equal(Color, fontImage.Color);
				i++;
			}
		}

		[Theory]
		[XamlInflatorData]
		internal void FontImageExtension_Negative(XamlInflator inflator)
		{
			var layout = new FontImageExtension(inflator);
			var tabs = ((IVisualTreeElement)layout).GetVisualChildren();

			foreach (var tab in tabs)
			{
				Tab myTab = (Tab)tab;
				if (myTab == null)
					continue;

				// Check that Icon is not exactly ImageSource type (but can be a subtype like FontImageSource)
				Assert.NotEqual(typeof(ImageSource), myTab.Icon.GetType());
			}
		}
	}
}