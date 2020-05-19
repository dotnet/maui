using NUnit.Framework;

namespace System.Maui.Core.UnitTests
{
	[TestFixture]
	public class TextDecorationUnitTests : BaseTestFixture
	{
		[TestCase("strikethrough", TextDecorations.Strikethrough)]
		[TestCase("underline", TextDecorations.Underline)]
		[TestCase("line-through", TextDecorations.Strikethrough)]
		[TestCase("none", TextDecorations.None)]
		[TestCase("strikethrough underline", TextDecorations.Underline | TextDecorations.Strikethrough)]
		[TestCase("underline strikethrough", TextDecorations.Underline | TextDecorations.Strikethrough)]
		[TestCase("underline line-through", TextDecorations.Underline | TextDecorations.Strikethrough)]
		[TestCase("line-through underline", TextDecorations.Underline | TextDecorations.Strikethrough)]

		public void TestTextDecorationConverter(string input, TextDecorations expected)
		{
			var converter = new TextDecorationConverter();
			Assert.AreEqual(converter.ConvertFromInvariantString(input), expected);
		}
	}
}