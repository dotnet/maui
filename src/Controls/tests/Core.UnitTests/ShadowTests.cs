using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{

	public class ShadowTests
	{
		[Fact]
		public void ShadowInitializesCorrectly()
		{
			// Arrange
			const float expectedOpacity = 1.0f;
			const float expectedRadius = 10.0f;
			var expectedOffset = new Point(10, 10);

			// Act
			var shadow = new Shadow
			{
				Offset = expectedOffset,
				Opacity = expectedOpacity,
				Radius = expectedRadius
			};

			// Assert
			Assert.Equal(expectedOffset, shadow.Offset);
			Assert.Equal(expectedOpacity, shadow.Opacity);
			Assert.Equal(expectedRadius, shadow.Radius);
		}
	}
}