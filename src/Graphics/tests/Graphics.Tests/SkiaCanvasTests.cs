using Microsoft.Maui.Graphics.Skia;
using SkiaSharp;
using Xunit;

namespace Microsoft.Maui.Graphics.Tests;

public class SkiaCanvasTests
{
	[Theory]
	[InlineData(25, 18, 30, 240, false, LineCap.Butt, -30, -210)]
	[InlineData(25, 18, 30, 240, false, LineCap.Round, -30, -210)]
	[InlineData(25, 18, 30, 240, false, LineCap.Square, -30, -210)]
	[InlineData(25, 18, 30, 240, true, LineCap.Butt, -30, 150)]
	[InlineData(25, 18, 30, 240, true, LineCap.Round, -30, 150)]
	[InlineData(25, 18, 30, 240, true, LineCap.Square, -30, 150)]
	[InlineData(25, 18, 0, 360, false, LineCap.Butt, 0, -360)]
	[InlineData(25, 18, 0, 360, false, LineCap.Round, 0, -360)]
	[InlineData(25, 18, 0, 360, false, LineCap.Square, 0, -360)]
	[InlineData(25, 18, 360, 0, true, LineCap.Butt, -360, 360)]
	[InlineData(25, 18, 360, 0, true, LineCap.Round, -360, 360)]
	[InlineData(25, 18, 360, 0, true, LineCap.Square, -360, 360)]
	[InlineData(0, 18, 30, 240, false, LineCap.Butt, -30, -210)]
	[InlineData(25, 0, 30, 240, false, LineCap.Butt, -30, -210)]
	public void DrawArcMatchesPathRendering(
		float width,
		float height,
		float startAngle,
		float endAngle,
		bool clockwise,
		LineCap lineCap,
		float expectedStartAngle,
		float expectedSweep)
	{
		using var actual = CreateBitmap();
		using var expected = CreateBitmap();
		using var actualPlatformCanvas = new SKCanvas(actual);
		using var expectedPlatformCanvas = new SKCanvas(expected);
		using var canvas = new SkiaCanvas { Canvas = actualPlatformCanvas };
		using var paint = new SKPaint
		{
			Color = SKColors.Red,
			IsAntialias = true,
			IsStroke = true,
			StrokeCap = GetStrokeCap(lineCap),
			StrokeMiter = CanvasDefaults.DefaultMiterLimit,
			StrokeWidth = 3,
		};
		using var path = new SKPath();

		canvas.StrokeColor = Colors.Red;
		canvas.StrokeLineCap = lineCap;
		canvas.StrokeSize = 3;
		canvas.DrawArc(5, 7, width, height, startAngle, endAngle, clockwise, false);

		path.AddArc(new SKRect(5, 7, 5 + width, 7 + height), expectedStartAngle, expectedSweep);
		expectedPlatformCanvas.DrawPath(path, paint);

		var shouldDrawPixels = width > 0 && height > 0 && expectedSweep != 0;
		Assert.Equal(shouldDrawPixels, HasPixels(expected));
		Assert.Equal(shouldDrawPixels, HasPixels(actual));
		Assert.Equal(expected.Bytes, actual.Bytes);
	}

	[Theory]
	[InlineData(25, 18, 30, 240, false, -30, -210)]
	[InlineData(25, 18, 30, 240, true, -30, 150)]
	[InlineData(25, 18, 0, 360, false, 0, -360)]
	[InlineData(25, 18, 0, 360, true, 0, 0)]
	[InlineData(25, 18, 360, 0, false, -360, 0)]
	[InlineData(25, 18, 360, 0, true, -360, 360)]
	[InlineData(0, 18, 30, 240, false, -30, -210)]
	[InlineData(25, 0, 30, 240, false, -30, -210)]
	public void FillArcMatchesPathRendering(
		float width,
		float height,
		float startAngle,
		float endAngle,
		bool clockwise,
		float expectedStartAngle,
		float expectedSweep)
	{
		using var actual = CreateBitmap();
		using var expected = CreateBitmap();
		using var actualPlatformCanvas = new SKCanvas(actual);
		using var expectedPlatformCanvas = new SKCanvas(expected);
		using var canvas = new SkiaCanvas { Canvas = actualPlatformCanvas };
		using var paint = new SKPaint
		{
			Color = SKColors.Red,
			IsAntialias = true,
			IsStroke = false,
		};
		using var path = new SKPath();

		canvas.FillColor = Colors.Red;
		canvas.FillArc(5, 7, width, height, startAngle, endAngle, clockwise);

		path.AddArc(new SKRect(5, 7, 5 + width, 7 + height), expectedStartAngle, expectedSweep);
		expectedPlatformCanvas.DrawPath(path, paint);

		var shouldDrawPixels = width > 0 && height > 0 && expectedSweep != 0;
		Assert.Equal(shouldDrawPixels, HasPixels(expected));
		Assert.Equal(shouldDrawPixels, HasPixels(actual));
		Assert.Equal(expected.Bytes, actual.Bytes);
	}

	private static SKBitmap CreateBitmap()
	{
		var bitmap = new SKBitmap(
			new SKImageInfo(40, 40, SKColorType.Rgba8888, SKAlphaType.Premul));
		bitmap.Erase(SKColors.Transparent);
		return bitmap;
	}

	private static bool HasPixels(SKBitmap bitmap)
	{
		foreach (var value in bitmap.Bytes)
		{
			if (value != 0)
				return true;
		}

		return false;
	}

	private static SKStrokeCap GetStrokeCap(LineCap lineCap) =>
		lineCap switch
		{
			LineCap.Round => SKStrokeCap.Round,
			LineCap.Square => SKStrokeCap.Square,
			_ => SKStrokeCap.Butt,
		};
}
