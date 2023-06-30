using Xunit;

namespace Microsoft.Maui.Graphics.Tests
{
	public class FontUnitTests
	{
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
