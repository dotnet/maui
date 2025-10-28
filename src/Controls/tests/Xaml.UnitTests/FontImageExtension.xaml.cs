using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class FontImageExtension : TabBar
	{
		public FontImageExtension() => InitializeComponent();
		public FontImageExtension(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		public static string FontFamily => "MyFontFamily";
		public static string Glyph => "MyGlyph";
		public static Color Color => Colors.Black;
		public static double Size = 50d;		class Tests
		{
			[InlineData(true), TestCase(false)]
			public void FontImageExtension_Positive(bool useCompiledXaml)
			{
				var layout = new FontImageExtension(useCompiledXaml);
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

			[InlineData(true), TestCase(false)]
			public void FontImageExtension_Negative(bool useCompiledXaml)
			{
				var layout = new FontImageExtension(useCompiledXaml);
				var tabs = ((IVisualTreeElement)layout).GetVisualChildren();

				foreach (var tab in tabs)
				{
					Tab myTab = (Tab)tab;
					if (myTab == null)
						continue;

					Assert.True(myTab.Icon, Is.Not.TypeOf<ImageSource>());
				}
			}
		}
	}
}