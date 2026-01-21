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
		global::Android.Graphics.RectF aRectF = rectF;

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
		var aRectF = (global::Android.Graphics.RectF)rectF;

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
		global::Android.Graphics.Rect aRect = rect;

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
		var aRect = (global::Android.Graphics.Rect)rect;

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
		global::Android.Graphics.PointF aPointF = pointF;

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
		var aPointF = (global::Android.Graphics.PointF)pointF;

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
		global::Android.Graphics.Point aPoint = point;

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
		var aPoint = (global::Android.Graphics.Point)point;

		Assert.Equal(point.X, aPoint.X);
		Assert.Equal(point.Y, aPoint.Y);
	}
}