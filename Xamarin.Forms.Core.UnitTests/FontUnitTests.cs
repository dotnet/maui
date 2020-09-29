using System.Globalization;

using NUnit.Framework;


namespace Xamarin.Forms.Core.UnitTests
{
	[TestFixture]
	public class FontUnitTests : BaseTestFixture
	{
		[Test]
		public void TestFontForSize()
		{
			var font = Font.OfSize("Foo", 12);
			Assert.AreEqual("Foo", font.FontFamily);
			Assert.AreEqual(12, font.FontSize);
			Assert.AreEqual((NamedSize)0, font.NamedSize);
		}

		[Test]
		public void TestFontForSizeDouble()
		{
			var font = Font.OfSize("Foo", 12.7);
			Assert.AreEqual("Foo", font.FontFamily);
			Assert.AreEqual(12.7, font.FontSize);
			Assert.AreEqual((NamedSize)0, font.NamedSize);
		}

		[Test]
		public void TestFontForNamedSize()
		{
			var font = Font.OfSize("Foo", NamedSize.Large);
			Assert.AreEqual("Foo", font.FontFamily);
			Assert.AreEqual(0, font.FontSize);
			Assert.AreEqual(NamedSize.Large, font.NamedSize);
		}

		[Test]
		public void TestSystemFontOfSize()
		{
			var font = Font.SystemFontOfSize(12);
			Assert.AreEqual(null, font.FontFamily);
			Assert.AreEqual(12, font.FontSize);
			Assert.AreEqual((NamedSize)0, font.NamedSize);

			font = Font.SystemFontOfSize(NamedSize.Medium);
			Assert.AreEqual(null, font.FontFamily);
			Assert.AreEqual(0, font.FontSize);
			Assert.AreEqual(NamedSize.Medium, font.NamedSize);
		}

		[TestCase("en-US"), TestCase("tr-TR"), TestCase("fr-FR")]
		public void CultureTestSystemFontOfSizeDouble(string culture)
		{
			System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo(culture);

			var font = Font.SystemFontOfSize(12.7);
			Assert.AreEqual(null, font.FontFamily);
			Assert.AreEqual(12.7, font.FontSize);
			Assert.AreEqual((NamedSize)0, font.NamedSize);

			font = Font.SystemFontOfSize(NamedSize.Medium);
			Assert.AreEqual(null, font.FontFamily);
			Assert.AreEqual(0, font.FontSize);
			Assert.AreEqual(NamedSize.Medium, font.NamedSize);
		}

		[Test]
		public void TestEquality()
		{
			var font1 = Font.SystemFontOfSize(12);
			var font2 = Font.SystemFontOfSize(12);

			Assert.True(font1 == font2);
			Assert.False(font1 != font2);

			font2 = Font.SystemFontOfSize(13);

			Assert.False(font1 == font2);
			Assert.True(font1 != font2);
		}

		[Test]
		public void TestHashCode()
		{
			var font1 = Font.SystemFontOfSize(12);
			var font2 = Font.SystemFontOfSize(12);

			Assert.True(font1.GetHashCode() == font2.GetHashCode());

			font2 = Font.SystemFontOfSize(13);

			Assert.False(font1.GetHashCode() == font2.GetHashCode());
		}

		[Test]
		public void TestEquals()
		{
			var font = Font.SystemFontOfSize(12);

			Assert.False(font.Equals(null));
			Assert.True(font.Equals(font));
			Assert.False(font.Equals("Font"));
			Assert.True(font.Equals(Font.SystemFontOfSize(12)));
		}

		[Test]
		public void TestFontConverter()
		{
			var converter = new FontTypeConverter();
			Assert.True(converter.CanConvertFrom(typeof(string)));
			Assert.AreEqual(Font.SystemFontOfSize(NamedSize.Medium), converter.ConvertFromInvariantString("Medium"));
			Assert.AreEqual(Font.SystemFontOfSize(42), converter.ConvertFromInvariantString("42"));
			Assert.AreEqual(Font.OfSize("Foo", NamedSize.Micro), converter.ConvertFromInvariantString("Foo, Micro"));
			Assert.AreEqual(Font.OfSize("Foo", 42), converter.ConvertFromInvariantString("Foo, 42"));
			Assert.AreEqual(Font.OfSize("Foo", 12.7), converter.ConvertFromInvariantString("Foo, 12.7"));
			Assert.AreEqual(Font.SystemFontOfSize(NamedSize.Large, FontAttributes.Bold), converter.ConvertFromInvariantString("Bold, Large"));
			Assert.AreEqual(Font.SystemFontOfSize(42, FontAttributes.Bold), converter.ConvertFromInvariantString("Bold, 42"));
			Assert.AreEqual(Font.OfSize("Foo", NamedSize.Medium), converter.ConvertFromInvariantString("Foo"));
			Assert.AreEqual(Font.OfSize("Foo", NamedSize.Large).WithAttributes(FontAttributes.Bold), converter.ConvertFromInvariantString("Foo, Bold, Large"));
			Assert.AreEqual(Font.OfSize("Foo", NamedSize.Large).WithAttributes(FontAttributes.Italic), converter.ConvertFromInvariantString("Foo, Italic, Large"));
			Assert.AreEqual(Font.OfSize("Foo", NamedSize.Large).WithAttributes(FontAttributes.Bold | FontAttributes.Italic), converter.ConvertFromInvariantString("Foo, Bold, Italic, Large"));
			Assert.AreEqual(Font.OfSize("Foo", 12).WithAttributes(FontAttributes.Bold), converter.ConvertFromInvariantString("Foo, Bold, 12"));
			Assert.AreEqual(Font.OfSize("Foo", 12.7).WithAttributes(FontAttributes.Bold), converter.ConvertFromInvariantString("Foo, Bold, 12.7"));
			Assert.AreEqual(Font.OfSize("Foo", 12).WithAttributes(FontAttributes.Italic), converter.ConvertFromInvariantString("Foo, Italic, 12"));
			Assert.AreEqual(Font.OfSize("Foo", 12.7).WithAttributes(FontAttributes.Italic), converter.ConvertFromInvariantString("Foo, Italic, 12.7"));
			Assert.AreEqual(Font.OfSize("Foo", 12).WithAttributes(FontAttributes.Bold | FontAttributes.Italic), converter.ConvertFromInvariantString("Foo, Bold, Italic, 12"));
			Assert.AreEqual(Font.OfSize("Foo", 12.7).WithAttributes(FontAttributes.Bold | FontAttributes.Italic), converter.ConvertFromInvariantString("Foo, Bold, Italic, 12.7"));
		}

		[Test]
		public void TestFontParsing()
		{
			var input = "PTM55FT#PTMono-Regular";
			var input2 = "PTM55FT.ttf#PTMono-Regular";
			var input3 = "CuteFont-Regular";

			var font1 = FontFile.FromString(input);
			var font2 = FontFile.FromString(input2);
			var font3 = FontFile.FromString(input3);

			Assert.AreEqual(font1.FileName, "PTM55FT");
			Assert.AreEqual(font1.PostScriptName, "PTMono-Regular");
			Assert.IsNull(font1.Extension);


			Assert.AreEqual(font2.FileName, "PTM55FT");
			Assert.AreEqual(font2.PostScriptName, "PTMono-Regular");
			Assert.AreEqual(font2.Extension, ".ttf");


			Assert.AreEqual(font3.FileName, "CuteFont-Regular");
			Assert.AreEqual(font3.PostScriptName, "CuteFont-Regular");
			Assert.IsNull(font3.Extension);

		}

	}
}