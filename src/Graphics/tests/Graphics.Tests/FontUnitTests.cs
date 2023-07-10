using Xunit;

namespace Microsoft.Maui.Graphics.Tests
{
	public class FontUnitTests
	{
		[Fact]
		public void TestDefaultFonts()
		{
			Assert.False(Font.Default.Equals(Font.DefaultBold));
		}

		[Fact]
		public void TestDefault()
		{
			Assert.True(Font.Default.Equals(Font.Default));
		}

		[Fact]
		public void TestDefaultBold()
		{
			Assert.True(Font.DefaultBold.Equals(Font.DefaultBold));
		}
	}
}
