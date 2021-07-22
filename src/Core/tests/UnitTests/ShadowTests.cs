using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.UnitTests
{
	[Category(TestCategory.Core)]
	public partial class ShadowTests
	{
		[Fact]
		public void ShadowInitializesCorrectly()
		{
			var expectedColor = Colors.Red;
			var expectedOffset = new Size(10, 10);
			var expectedOpacity = 1.0f;
			var expectedRadius = 10.0f;

			var shadow = new Shadow
			{
				Color = expectedColor,
				Offset = expectedOffset,
				Opacity = expectedOpacity,
				Radius = expectedRadius
			};

			Assert.Equal(expectedColor, shadow.Color);
			Assert.Equal(expectedOffset, shadow.Offset);
			Assert.Equal(expectedOpacity, shadow.Opacity);
			Assert.Equal(expectedRadius, shadow.Radius);
		}
	}
}