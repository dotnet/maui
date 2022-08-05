using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{

	public class TextDecorationUnitTests : BaseTestFixture
	{
		[Theory]
		[InlineData("strikethrough", TextDecorations.Strikethrough)]
		[InlineData("underline", TextDecorations.Underline)]
		[InlineData("line-through", TextDecorations.Strikethrough)]
		[InlineData("none", TextDecorations.None)]
		[InlineData("strikethrough underline", TextDecorations.Underline | TextDecorations.Strikethrough)]
		[InlineData("underline strikethrough", TextDecorations.Underline | TextDecorations.Strikethrough)]
		[InlineData("underline line-through", TextDecorations.Underline | TextDecorations.Strikethrough)]
		[InlineData("line-through underline", TextDecorations.Underline | TextDecorations.Strikethrough)]
		public void TestTextDecorationConverter(string input, TextDecorations expected)
		{
			var converter = new TextDecorationConverter();
			Assert.Equal(converter.ConvertFromInvariantString(input), expected);
		}
	}
}