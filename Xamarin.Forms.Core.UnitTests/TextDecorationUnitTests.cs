using System.Globalization;

using NUnit.Framework;


namespace Xamarin.Forms.Core.UnitTests
{
	[TestFixture]
	public class TextDecorationUnitTests : BaseTestFixture
	{
		[Test]
		public void TestTextDecorationConverter()
		{
			var converter = new TextDecorationConverter();
			TextDecorations both = TextDecorations.Strikethrough;
			both |= TextDecorations.Underline;
			Assert.True(converter.CanConvertFrom(typeof(string)));
			Assert.AreEqual(TextDecorations.Strikethrough, converter.ConvertFromInvariantString("strikethrough"));
			Assert.AreEqual(TextDecorations.Underline, converter.ConvertFromInvariantString("underline"));
			Assert.AreEqual(TextDecorations.Strikethrough, converter.ConvertFromInvariantString("line-through"));
			Assert.AreEqual(TextDecorations.None, converter.ConvertFromInvariantString("none"));
			Assert.AreEqual(both, converter.ConvertFromInvariantString("strikethrough underline"));
			Assert.AreEqual(both, converter.ConvertFromInvariantString("underline strikethrough"));
			Assert.AreEqual(both, converter.ConvertFromInvariantString("underline line-through"));
			Assert.AreEqual(both, converter.ConvertFromInvariantString("line-through underline"));
		}
	}
}
