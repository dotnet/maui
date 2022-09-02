using System.Globalization;

using Xunit;


namespace Microsoft.Maui.Controls.Core.UnitTests
{

	public class FontUnitTests : BaseTestFixture
	{
		[Fact]
		public void TestFontForSize()
		{
			var font = Font.OfSize("Foo", 12);
			Assert.Equal("Foo", font.Family);
			Assert.Equal(12, font.Size);
		}

		[Fact]
		public void TestFontForSizeDouble()
		{
			var font = Font.OfSize("Foo", 12.7);
			Assert.Equal("Foo", font.Family);
			Assert.Equal(12.7, font.Size);
		}

		[Fact]
		public void TestFontForNamedSize()
		{
			var size = Device.GetNamedSize(NamedSize.Large, null, false);
			var font = Font.OfSize("Foo", size);
			Assert.Equal("Foo", font.Family);
			Assert.Equal(size, font.Size);
		}

		[Fact]
		public void TestSystemFontOfSize()
		{
			var font = Font.SystemFontOfSize(12);
			Assert.Null(font.Family);
			Assert.Equal(12, font.Size);


			var size = Device.GetNamedSize(NamedSize.Medium, null, false);
			font = Font.SystemFontOfSize(size);
			Assert.Null(font.Family);
			Assert.Equal(size, font.Size);
		}

		[Theory, InlineData("en-US"), InlineData("tr-TR"), InlineData("fr-FR")]
		public void CultureTestSystemFontOfSizeDouble(string culture)
		{
			System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo(culture);

			var font = Font.SystemFontOfSize(12.7);
			Assert.Null(font.Family);
			Assert.Equal(12.7, font.Size);

			var size = Device.GetNamedSize(NamedSize.Medium, null, false);
			font = Font.SystemFontOfSize(size);
			Assert.Null(font.Family);
			Assert.Equal(size, font.Size);
		}

		[Fact]
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

		[Fact]
		public void TestHashCode()
		{
			var font1 = Font.SystemFontOfSize(12);
			var font2 = Font.SystemFontOfSize(12);

			Assert.True(font1.GetHashCode() == font2.GetHashCode());

			font2 = Font.SystemFontOfSize(13);

			Assert.False(font1.GetHashCode() == font2.GetHashCode());
		}

		[Fact]
		public void TestEquals()
		{
			var font = Font.SystemFontOfSize(12);

			Assert.False(font.Equals(null));
			Assert.True(font.Equals(font));
			Assert.False(font.Equals("Font"));
			Assert.True(font.Equals(Font.SystemFontOfSize(12)));
		}

		[Fact]
		public void TestFontParsing()
		{
			var input = "PTM55FT#PTMono-Regular";
			var input2 = "PTM55FT.ttf#PTMono-Regular";
			var input3 = "CuteFont-Regular";

			var font1 = FontFile.FromString(input);
			var font2 = FontFile.FromString(input2);
			var font3 = FontFile.FromString(input3);

			Assert.Equal("PTM55FT", font1.FileName);
			Assert.Equal("PTMono-Regular", font1.PostScriptName);
			Assert.Null(font1.Extension);


			Assert.Equal("PTM55FT", font2.FileName);
			Assert.Equal("PTMono-Regular", font2.PostScriptName);
			Assert.Equal(".ttf", font2.Extension);


			Assert.Equal("CuteFont-Regular", font3.FileName);
			Assert.Equal("CuteFont-Regular", font3.PostScriptName);
			Assert.Null(font3.Extension);

		}
	}
}
