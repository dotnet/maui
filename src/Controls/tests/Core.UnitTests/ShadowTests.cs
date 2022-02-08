using Microsoft.Maui.Graphics;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	[TestFixture]
	public class ShadowTests
	{
		[Test]
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
			Assert.AreEqual(expectedOffset, shadow.Offset);
			Assert.AreEqual(expectedOpacity, shadow.Opacity);
			Assert.AreEqual(expectedRadius, shadow.Radius);
		}
	}
}