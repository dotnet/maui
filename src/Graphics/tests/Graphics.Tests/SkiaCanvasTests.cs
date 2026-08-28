using Microsoft.Maui.Graphics.Skia;
using SkiaSharp;
using Xunit;

namespace Microsoft.Maui.Graphics.Tests;

public class SkiaCanvasTests
{
	[Theory]
	[InlineData(false, -30, -210)]
	[InlineData(true, -30, 150)]
	public void DrawArcMatchesPathRendering(bool clockwise, float expectedStartAngle, float expectedSweep)
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
			StrokeMiter = CanvasDefaults.DefaultMiterLimit,
			StrokeWidth = 3,
		};
		using var path = new SKPath();

		canvas.StrokeColor = Colors.Red;
		canvas.StrokeSize = 3;
		canvas.DrawArc(5, 7, 25, 18, 30, 240, clockwise, false);

		path.AddArc(new SKRect(5, 7, 30, 25), expectedStartAngle, expectedSweep);
		expectedPlatformCanvas.DrawPath(path, paint);

		Assert.Equal(expected.Bytes, actual.Bytes);
	}

	[Theory]
	[InlineData(false, -30, -210)]
	[InlineData(true, -30, 150)]
	public void FillArcMatchesPathRendering(bool clockwise, float expectedStartAngle, float expectedSweep)
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
		canvas.FillArc(5, 7, 25, 18, 30, 240, clockwise);

		path.AddArc(new SKRect(5, 7, 30, 25), expectedStartAngle, expectedSweep);
		expectedPlatformCanvas.DrawPath(path, paint);

		Assert.Equal(expected.Bytes, actual.Bytes);
	}

	private static SKBitmap CreateBitmap()
	{
		var bitmap = new SKBitmap(
			new SKImageInfo(40, 40, SKColorType.Rgba8888, SKAlphaType.Premul));
		bitmap.Erase(SKColors.Transparent);
		return bitmap;
	}
}
