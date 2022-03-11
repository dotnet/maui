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
			var expectedColor = new SolidPaint(Colors.Red);
			var expectedOffset = new Point(10, 10);
			var expectedOpacity = 1.0f;
			var expectedRadius = 10.0f;

			var shadow = new ShadowStub
			{
				Paint = expectedColor,
				Offset = expectedOffset,
				Opacity = expectedOpacity,
				Radius = expectedRadius
			};

			Assert.Equal(expectedColor.Color, shadow.Paint.ToColor());
			Assert.Equal(expectedOffset, shadow.Offset);
			Assert.Equal(expectedOpacity, shadow.Opacity);
			Assert.Equal(expectedRadius, shadow.Radius);
		}
	}
}