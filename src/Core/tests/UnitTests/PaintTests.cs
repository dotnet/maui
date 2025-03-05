using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.UnitTests
{
	[Category(TestCategory.Core)]
	public class PaintTests
	{
		[Fact]
		public void TestLinearGradientPaintConstructor()
		{
			LinearGradientPaint linearGradientPaint = new LinearGradientPaint();

			Assert.Equal(1.0d, linearGradientPaint.EndPoint.X);
			Assert.Equal(1.0d, linearGradientPaint.EndPoint.Y);
		}

		[Fact]
		public void TestLinearGradientPaintStartColor()
		{
			LinearGradientPaint linearGradientPaint = new LinearGradientPaint();

			Assert.NotNull(linearGradientPaint.StartColor);
		}

		[Fact]
		public void TestLinearGradientPaintEndColor()
		{
			LinearGradientPaint linearGradientPaint = new LinearGradientPaint();

			Assert.NotNull(linearGradientPaint.EndColor);
		}

		[Fact]
		public void TestNullOrEmptyLinearGradientPaint()
		{
			LinearGradientPaint nullLinearGradientPaint = null;
			Assert.True(nullLinearGradientPaint.IsNullOrEmpty());

			LinearGradientPaint emptyLinearGradientPaint = new LinearGradientPaint();
			Assert.False(emptyLinearGradientPaint.IsNullOrEmpty());

			PaintGradientStop[] linearGradientStops =
			{
				new PaintGradientStop(0.1f, Colors.Orange),
				new PaintGradientStop(0.8f, Colors.Red)
			};

			LinearGradientPaint linearGradientPaint = new LinearGradientPaint
			{
				StartPoint = new Point(0, 0),
				EndPoint = new Point(1, 0),
				GradientStops = linearGradientStops
			};

			Assert.False(linearGradientPaint.IsNullOrEmpty());
		}
	}
}