using Xunit;

namespace Microsoft.Maui.Graphics.Tests
{
	public class FontUnitTests
	{
		[Fact]
		public void TestEqualsDefault()
		{
			Assert.True(Font.Default.Equals(Font.Default));
		}

		[Fact]
		public void TestEqualsDefaultBold()
		{
			Assert.True(Font.DefaultBold.Equals(Font.DefaultBold));
		}

		[Fact]
		public void TestEqualsDefaultFonts()
		{
			Assert.False(Font.Default.Equals(Font.DefaultBold));
		}

		[Fact]
		public void TestEqualsArialFont()
		{
			var font1 = new Font("Arial");
			var font2 = new Font("Arial");
			Assert.True(font1.Equals(font2));
		}
	}
}
