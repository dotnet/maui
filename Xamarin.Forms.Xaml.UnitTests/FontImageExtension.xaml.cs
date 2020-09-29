using NUnit.Framework;

using Xamarin.Forms.Core.UnitTests;

namespace Xamarin.Forms.Xaml.UnitTests
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
		public static Color Color => Color.Black;
		public static double Size = 50d;

		[TestFixture]
		class Tests
		{
			[SetUp] public void Setup() => Device.PlatformServices = new MockPlatformServices();
			[TearDown] public void TearDown() => Device.PlatformServices = null;

			[TestCase(true), TestCase(false)]
			public void FontImageExtension_Positive(bool useCompiledXaml)
			{
				var layout = new FontImageExtension(useCompiledXaml);
				var tabs = layout.AllChildren;

				int i = 0;
				foreach (var tab in tabs)
				{
					Tab myTab = (Tab)tab;
					if (myTab == null)
						continue;

					Assert.That(myTab.Icon, Is.TypeOf<FontImageSource>());

					var fontImage = (FontImageSource)myTab.Icon;
					Assert.AreEqual(FontFamily, fontImage.FontFamily);
					Assert.AreEqual(Glyph, fontImage.Glyph);

					if (i == 3)
						Assert.AreEqual(30d, fontImage.Size);
					else
						Assert.AreEqual(Size, fontImage.Size);

					Assert.AreEqual(Color, fontImage.Color);
					i++;
				}
			}

			[TestCase(true), TestCase(false)]
			public void FontImageExtension_Negative(bool useCompiledXaml)
			{
				var layout = new FontImageExtension(useCompiledXaml);
				var tabs = layout.AllChildren;

				foreach (var tab in tabs)
				{
					Tab myTab = (Tab)tab;
					if (myTab == null)
						continue;

					Assert.That(myTab.Icon, Is.Not.TypeOf<ImageSource>());
				}
			}
		}
	}
}