using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Java.Interop;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Platform;
using Xunit;

namespace Microsoft.Maui.DeviceTests;

[Category(TestCategory.Graphics)]
public partial class GraphicsTests : TestBase
{
	[Theory]
	[InlineData("DrawStringPoint")]
	[InlineData("DrawStringRectangle")]
	[InlineData("GetStringSize")]
	[InlineData("GetStringSizeWithAlignment")]
	public async Task TextOperationsDisposeTemporaryJavaPeers(string operation)
	{
		await InvokeOnMainThreadAsync(() =>
		{
			using var bitmap = global::Android.Graphics.Bitmap.CreateBitmap(200, 80, global::Android.Graphics.Bitmap.Config.Argb8888!);
			using var nativeCanvas = new global::Android.Graphics.Canvas(bitmap);
			using var canvas = new PlatformCanvas { Canvas = nativeCanvas, FontSize = 18 };
			var sizeService = new PlatformStringSizeService();
			var font = Microsoft.Maui.Graphics.Font.Default;

			Action run = operation switch
			{
				"DrawStringPoint" => () => canvas.DrawString("Hello MAUI", 0, 20, HorizontalAlignment.Left),
				"DrawStringRectangle" => () => canvas.DrawString("Hello MAUI", 0, 0, 200, 80, HorizontalAlignment.Left, VerticalAlignment.Top),
				"GetStringSize" => () => sizeService.GetStringSize("Hello MAUI", font, 18),
				"GetStringSizeWithAlignment" => () => sizeService.GetStringSize("Hello MAUI", font, 18, HorizontalAlignment.Center, VerticalAlignment.Center),
				_ => throw new ArgumentOutOfRangeException(nameof(operation))
			};

			// Warm up canvas-owned paint, then check explicit cleanup rather than GC or process memory.
			run();
			var existingPeers = GetTextPeers();
			for (int i = 0; i < 20; i++)
				run();

			var remainingPeers = GetTextPeers();
			remainingPeers.ExceptWith(existingPeers);
			try
			{
				Assert.Empty(remainingPeers);
			}
			finally
			{
				foreach (var peer in remainingPeers)
					peer.Dispose();
			}
		});

		static HashSet<IJavaPeerable> GetTextPeers()
		{
			var peers = new HashSet<IJavaPeerable>(ReferenceEqualityComparer.Instance);
			foreach (var entry in JniEnvironment.Runtime.ValueManager.GetSurfacedPeers())
			{
				if (entry.SurfacedPeer.TryGetTarget(out var peer) &&
					peer.PeerReference.IsValid &&
					peer is global::Android.Text.StaticLayout or global::Android.Text.TextPaint or global::Android.Text.StaticLayout.Builder)
				{
					peers.Add(peer);
				}
			}
			return peers;
		}
	}

	[Fact]
	public void RadialGradientWithZeroSizeDoesNotThrow()
	{
		var paint = new RadialGradientPaintStub(Colors.Red, Colors.Blue)
		{
			Center = new Point(0.5, 0.5),
			Radius = 0.5
		};

		var context = global::Android.App.Application.Context;
		using var drawable = new Microsoft.Maui.Graphics.MauiDrawable(context);

		drawable.SetBounds(0, 0, 0, 0);
		drawable.SetBackground(paint);

		using var bitmap = global::Android.Graphics.Bitmap.CreateBitmap(1, 1, global::Android.Graphics.Bitmap.Config.Argb8888!);
		using var canvas = new global::Android.Graphics.Canvas(bitmap);

		var ex = Record.Exception(() => drawable.Draw(canvas));
		Assert.Null(ex);
	}

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