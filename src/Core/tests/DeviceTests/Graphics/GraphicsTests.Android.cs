using Microsoft.Maui.DeviceTests.Stubs;
using Xunit;

namespace Microsoft.Maui.DeviceTests;

[Category(TestCategory.Graphics)]
public partial class GraphicsTests : TestBase
{
	[Theory]
	[InlineData(0, 0, 0, 0)]
	[InlineData(10, 10, 100, 100)]
	[InlineData(0, 0, 10, 100)]
	[InlineData(0, 0, 100, 10)]
	[InlineData(50, 100, 100, 50)]
	[InlineData(15, 15, 50, 50)]
	public void RectFImplicitConversionTest(float x, float y, float width, float height)
	{
		var rectF = new Microsoft.Maui.Graphics.RectF(x, y, width, height);
		Android.Graphics.RectF aRectF = rectF;

		Assert.Equal(rectF.X, aRectF.Left);
		Assert.Equal(rectF.Y, aRectF.Top);
		Assert.Equal(rectF.Width, aRectF.Width());
		Assert.Equal(rectF.Height, aRectF.Height());
	}

	[Theory]
	[InlineData(0, 0, 0, 0)]
	[InlineData(10, 10, 100, 100)]
	[InlineData(0, 0, 10, 100)]
	[InlineData(0, 0, 100, 10)]
	[InlineData(50, 100, 100, 50)]
	[InlineData(15, 15, 50, 50)]
	public void RectFExplicitConversionTest(float x, float y, float width, float height)
	{
		var rectF = new Microsoft.Maui.Graphics.RectF(x, y, width, height);
		var aRectF = (Android.Graphics.RectF)rectF;

		Assert.Equal(rectF.X, aRectF.Left);
		Assert.Equal(rectF.Y, aRectF.Top);
		Assert.Equal(rectF.Width, aRectF.Width());
		Assert.Equal(rectF.Height, aRectF.Height());
	}

	[Theory]
	[InlineData(0, 0, 0, 0)]
	[InlineData(10, 10, 100, 100)]
	[InlineData(0, 0, 10, 100)]
	[InlineData(0, 0, 100, 10)]
	[InlineData(50, 100, 100, 50)]
	[InlineData(15, 15, 50, 50)]
	public void RectImplicitConversionTest(double x, double y, double width, double height)
	{
		var rect = new Microsoft.Maui.Graphics.Rect(x, y, width, height);
		Android.Graphics.Rect aRect = rect;

		Assert.Equal(rect.X, aRect.Left);
		Assert.Equal(rect.Y, aRect.Top);
		Assert.Equal(rect.Width, aRect.Width());
		Assert.Equal(rect.Height, aRect.Height());
	}

	[Theory]
	[InlineData(0, 0, 0, 0)]
	[InlineData(10, 10, 100, 100)]
	[InlineData(0, 0, 10, 100)]
	[InlineData(0, 0, 100, 10)]
	[InlineData(50, 100, 100, 50)]
	[InlineData(15, 15, 50, 50)]
	public void RectExplicitConversionTest(float x, float y, float width, float height)
	{
		var rect = new Microsoft.Maui.Graphics.Rect(x, y, width, height);
		var aRect = (Android.Graphics.Rect)rect;

		Assert.Equal(rect.X, aRect.Left);
		Assert.Equal(rect.Y, aRect.Top);
		Assert.Equal(rect.Width, aRect.Width());
		Assert.Equal(rect.Height, aRect.Height());
	}

	[Theory]
	[InlineData(0, 0)]
	[InlineData(100, 100)]
	[InlineData(10, 100)]
	[InlineData(100, 10)]
	[InlineData(50, 100)]
	[InlineData(15, 15)]
	public void PointFImplicitConversionTest(float x, float y)
	{
		var pointF = new Microsoft.Maui.Graphics.PointF(x, y);
		Android.Graphics.PointF aPointF = pointF;

		Assert.Equal(pointF.X, aPointF.X);
		Assert.Equal(pointF.Y, aPointF.Y);
	}

	[Theory]
	[InlineData(0, 0)]
	[InlineData(100, 100)]
	[InlineData(10, 100)]
	[InlineData(100, 10)]
	[InlineData(50, 100)]
	[InlineData(15, 15)]
	public void PointFExplicitConversionTest(float x, float y)
	{
		var pointF = new Microsoft.Maui.Graphics.PointF(x, y);
		var aPointF = (Android.Graphics.PointF)pointF;

		Assert.Equal(pointF.X, aPointF.X);
		Assert.Equal(pointF.Y, aPointF.Y);
	}

	[Theory]
	[InlineData(0, 0)]
	[InlineData(100, 100)]
	[InlineData(10, 100)]
	[InlineData(100, 10)]
	[InlineData(50, 100)]
	[InlineData(15, 15)]
	public void PointImplicitConversionTest(float x, float y)
	{
		var point = new Microsoft.Maui.Graphics.Point(x, y);
		Android.Graphics.Point aPoint = point;

		Assert.Equal(point.X, aPoint.X);
		Assert.Equal(point.Y, aPoint.Y);
	}

	[Theory]
	[InlineData(0, 0)]
	[InlineData(100, 100)]
	[InlineData(10, 100)]
	[InlineData(100, 10)]
	[InlineData(50, 100)]
	[InlineData(15, 15)]
	public void PointExplicitConversionTest(float x, float y)
	{
		var point = new Microsoft.Maui.Graphics.Point(x, y);
		var aPoint = (Android.Graphics.Point)point;

		Assert.Equal(point.X, aPoint.X);
		Assert.Equal(point.Y, aPoint.Y);
	}

	[Theory]
	[InlineData("#FF0000")]
	[InlineData("#00FF00")]
	[InlineData("#0000FF")]
	public void SolidPaintTest(string hexColor)
	{
		var color = Color.FromArgb(hexColor);
		var solidPaint = new SolidPaint(color);

		Assert.True(solidPaint.IsSolid());
	}

	[Fact]
	public void NullSolidPaintTest()
	{
		Color nullColor = null;
		var solidPaintNullColor = new SolidPaint(nullColor);

		Assert.False(solidPaintNullColor.IsSolid());

		SolidPaint nullSolidPaint = null;

		Assert.False(nullSolidPaint.IsSolid());
	}

	[Theory]
	[InlineData("#FF0000", "#00FF00")]
	[InlineData("#00FF00", "#0000FF")]
	[InlineData("#0000FF", "#FF0000")]
	public void LinearGradientPaintTest(string startHexColor, string endHexColor)
	{
		var startColor = Color.FromArgb(startHexColor);
		var endColor = Color.FromArgb(endHexColor);
		var linearGradientPaint = new LinearGradientPaintStub(startColor, endColor);

		Assert.True(linearGradientPaint.IsSolid());
	}

	[Fact]
	public void NullLinearGradientPaintTest()
	{
		LinearGradientPaintStub nullLinearGradientPaint = null;

		Assert.False(nullLinearGradientPaint.IsSolid());
	}

	[Theory]
	[InlineData("#FF0000", "#00FF00")]
	[InlineData("#00FF00", "#0000FF")]
	[InlineData("#0000FF", "#FF0000")]
	public void RadialGradientPaintTest(string startHexColor, string endHexColor)
	{
		var startColor = Color.FromArgb(startHexColor);
		var endColor = Color.FromArgb(endHexColor);
		var radialGradientPaint = new RadialGradientPaintStub(startColor, endColor);

		Assert.True(radialGradientPaint.IsSolid());
	}

	[Fact]
	public void NullRadialGradientPaintTest()
	{
		RadialGradientPaintStub nullRadialGradientPaint = null;

		Assert.False(nullRadialGradientPaint.IsSolid());
	}
}