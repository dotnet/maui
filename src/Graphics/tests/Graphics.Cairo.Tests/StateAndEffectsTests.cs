using Xunit;

namespace Microsoft.Maui.Graphics.Cairo.Tests
{
	public class StateAndEffectsTests
	{
		[Fact]
		public void TranslateScaleAndRotateTransformDrawing()
		{
			using var s = new CairoTestSurface();
			s.Canvas.FillColor = Colors.Red;
			s.Canvas.Translate(10, 20);
			s.Canvas.Scale(2, 3);
			s.Canvas.FillRectangle(0, 0, 10, 10);
			Assert.Equal((10, 20, 30, 50), s.PaintedBounds());

			using var r = new CairoTestSurface();
			r.Canvas.FillColor = Colors.Red;
			// a 20x20 square rotated by 90 degrees about its center stays in place
			r.Canvas.Rotate(90, 50, 50);
			r.Canvas.FillRectangle(40, 20, 20, 60);
			var bounds = r.PaintedBounds();
			Assert.NotNull(bounds);
			Assert.Equal((20, 40, 80, 60), bounds);
		}

		[Fact]
		public void SaveAndRestoreStateRestoresTransformClipAndColors()
		{
			using var s = new CairoTestSurface();
			s.Canvas.FillColor = Colors.Red;
			s.Canvas.SaveState();
			s.Canvas.FillColor = Colors.Blue;
			s.Canvas.Translate(50, 0);
			s.Canvas.ClipRectangle(0, 0, 10, 10);
			s.Canvas.FillRectangle(0, 0, 100, 100);
			Assert.True(s.Canvas.RestoreState());

			s.Canvas.FillRectangle(0, 50, 20, 20);

			Assert.Equal((255, 0, 0, 255), s.Pixel(55, 5));
			Assert.True(s.IsTransparent(70, 5));
			Assert.Equal((255, 255, 0, 0), s.Pixel(10, 60));
		}

		[Fact]
		public void RestoreStateWithoutSaveReturnsFalse()
		{
			using var s = new CairoTestSurface();
			Assert.False(s.Canvas.RestoreState());
		}

		[Fact]
		public void ResetStateRewindsEveryNestedSave()
		{
			using var s = new CairoTestSurface();
			s.Canvas.SaveState();
			s.Canvas.Translate(30, 30);
			s.Canvas.SaveState();
			s.Canvas.Translate(30, 30);
			s.Canvas.ResetState();

			s.Canvas.FillColor = Colors.Red;
			s.Canvas.FillRectangle(0, 0, 10, 10);
			Assert.Equal((0, 0, 10, 10), s.PaintedBounds());
		}

		[Fact]
		public void ClipRectangleLimitsDrawing()
		{
			using var s = new CairoTestSurface();
			s.Canvas.ClipRectangle(10, 10, 20, 20);
			s.Canvas.FillColor = Colors.Red;
			s.Canvas.FillRectangle(0, 0, 100, 100);
			Assert.Equal((10, 10, 30, 30), s.PaintedBounds());
		}

		[Fact]
		public void SubtractFromClipPunchesAHole()
		{
			using var s = new CairoTestSurface();
			s.Canvas.SubtractFromClip(30, 30, 40, 40);
			s.Canvas.FillColor = Colors.Red;
			s.Canvas.FillRectangle(0, 0, 100, 100);

			Assert.True(s.IsTransparent(50, 50));
			Assert.False(s.IsTransparent(10, 10));
			Assert.False(s.IsTransparent(90, 50));
			Assert.False(s.IsTransparent(50, 90));
		}

		[Fact]
		public void SubtractFromClipAfterClipRectangleKeepsTheOuterClip()
		{
			using var s = new CairoTestSurface();
			s.Canvas.ClipRectangle(10, 10, 60, 60);
			s.Canvas.SubtractFromClip(30, 30, 20, 20);
			s.Canvas.FillColor = Colors.Red;
			s.Canvas.FillRectangle(0, 0, 100, 100);

			Assert.True(s.IsTransparent(40, 40));
			Assert.False(s.IsTransparent(15, 15));
			Assert.True(s.IsTransparent(80, 80));
			Assert.True(s.IsTransparent(5, 5));
		}

		[Fact]
		public void ClipPathHonoursTheWindingMode()
		{
			var path = new PathF();
			path.AppendRectangle(10, 10, 80, 80);
			path.AppendRectangle(30, 30, 40, 40);

			using var s = new CairoTestSurface();
			s.Canvas.ClipPath(path, WindingMode.EvenOdd);
			s.Canvas.FillColor = Colors.Red;
			s.Canvas.FillRectangle(0, 0, 100, 100);

			Assert.True(s.IsTransparent(50, 50));
			Assert.False(s.IsTransparent(20, 20));
			Assert.True(s.IsTransparent(5, 5));
		}

		[Fact]
		public void LinearGradientRunsAcrossTheRectangle()
		{
			var paint = RedToBlue();

			using var s = new CairoTestSurface();
			s.Canvas.SetFillPaint(paint, new RectF(0, 0, 100, 100));
			s.Canvas.FillRectangle(0, 0, 100, 100);

			var left = s.Pixel(2, 50);
			var middle = s.Pixel(50, 50);
			var right = s.Pixel(97, 50);
			Assert.True(left.R > 230 && left.B < 25);
			Assert.InRange(middle.R, 110, 145);
			Assert.InRange(middle.B, 110, 145);
			Assert.True(right.B > 230 && right.R < 25);
		}

		[Fact]
		public void RadialGradientRunsFromTheCenter()
		{
			var paint = new RadialGradientPaint(Stops(), new Point(0.5, 0.5), 0.5);

			using var s = new CairoTestSurface();
			s.Canvas.SetFillPaint(paint, new RectF(0, 0, 100, 100));
			s.Canvas.FillRectangle(0, 0, 100, 100);

			Assert.True(s.Pixel(50, 50).R > 240);
			Assert.True(s.Pixel(2, 2).B > 200);
		}

		[Fact]
		public void FillColorReplacesAGradient()
		{
			using var s = new CairoTestSurface();
			s.Canvas.SetFillPaint(RedToBlue(), new RectF(0, 0, 100, 100));
			s.Canvas.FillColor = Colors.Green;
			s.Canvas.FillRectangle(0, 0, 100, 100);

			Assert.Equal(s.Pixel(2, 50), s.Pixel(97, 50));
			Assert.Equal(0, s.Pixel(2, 50).R);
		}

		[Fact]
		public void GradientSurvivesSaveAndRestore()
		{
			using var s = new CairoTestSurface();
			s.Canvas.SetFillPaint(RedToBlue(), new RectF(0, 0, 100, 100));
			s.Canvas.SaveState();
			s.Canvas.FillColor = Colors.Green;
			s.Canvas.RestoreState();
			s.Canvas.FillRectangle(0, 0, 100, 100);

			Assert.True(s.Pixel(2, 50).R > 230);
			Assert.True(s.Pixel(97, 50).B > 230);
		}

		[Fact]
		public void PatternPaintTilesThePattern()
		{
			var pattern = new TestPattern();
			var paint = new PatternPaint { Pattern = pattern };

			using var s = new CairoTestSurface();
			s.Canvas.SetFillPaint(paint, new RectF(0, 0, 100, 100));
			s.Canvas.FillRectangle(0, 0, 100, 100);

			// 10x10 tile, red in the 5x5 top-left quarter
			Assert.Equal((255, 255, 0, 0), s.Pixel(2, 2));
			Assert.True(s.IsTransparent(7, 7));
			Assert.Equal((255, 255, 0, 0), s.Pixel(52, 32));
			Assert.True(s.IsTransparent(57, 37));
		}

		[Fact]
		public void ImagePaintTilesTheImage()
		{
			using var tile = new CairoTestSurface(10, 10);
			tile.Canvas.FillColor = Colors.Blue;
			tile.Canvas.FillRectangle(0, 0, 5, 10);
			using var image = new CairoImage(tile.Surface, ownsSurface: false);

			using var s = new CairoTestSurface();
			s.Canvas.SetFillPaint(new ImagePaint { Image = image }, new RectF(0, 0, 100, 100));
			s.Canvas.FillRectangle(0, 0, 100, 100);

			Assert.Equal((255, 0, 0, 255), s.Pixel(2, 50));
			Assert.True(s.IsTransparent(7, 50));
			Assert.Equal((255, 0, 0, 255), s.Pixel(82, 50));
		}

		[Fact]
		public void HardShadowIsOffsetByTheRequestedAmount()
		{
			using var s = new CairoTestSurface();
			s.Canvas.SetShadow(new SizeF(20, 10), 0, Colors.Black);
			s.Canvas.FillColor = Colors.Red;
			s.Canvas.FillRectangle(10, 10, 30, 30);

			// the rectangle on top, the black silhouette 20 right and 10 down
			Assert.Equal((255, 255, 0, 0), s.Pixel(20, 20));
			Assert.Equal((255, 0, 0, 0), s.Pixel(55, 45));
			Assert.True(s.IsTransparent(55, 15));
			Assert.True(s.IsTransparent(45, 55));
		}

		[Fact]
		public void BlurredShadowFadesOut()
		{
			using var s = new CairoTestSurface();
			s.Canvas.SetShadow(new SizeF(0, 0), 0, Colors.Black);
			s.Canvas.SetShadow(new SizeF(1, 1), 12, Colors.Black);
			s.Canvas.FillColor = Colors.White;
			s.Canvas.FillRectangle(30, 30, 40, 40);

			var edge = s.Alpha(75, 50);
			Assert.InRange(edge, 1, 254);
			Assert.True(s.Alpha(79, 50) < edge);
			Assert.Equal(255, s.Alpha(50, 50));
		}

		[Fact]
		public void ZeroShadowRemovesTheShadow()
		{
			using var s = new CairoTestSurface();
			s.Canvas.SetShadow(new SizeF(20, 20), 0, Colors.Black);
			s.Canvas.SetShadow(SizeF.Zero, 0, null);
			s.Canvas.FillColor = Colors.Red;
			s.Canvas.FillRectangle(10, 10, 30, 30);
			Assert.Equal((10, 10, 40, 40), s.PaintedBounds());
		}

		[Fact]
		public void ShadowIsRestoredWithTheState()
		{
			using var s = new CairoTestSurface();
			s.Canvas.SaveState();
			s.Canvas.SetShadow(new SizeF(30, 0), 0, Colors.Black);
			s.Canvas.RestoreState();
			s.Canvas.FillColor = Colors.Red;
			s.Canvas.FillRectangle(10, 10, 20, 20);
			Assert.Equal((10, 10, 30, 30), s.PaintedBounds());
		}

		[Fact]
		public void BlurSoftensTheEdges()
		{
			using var sharp = new CairoTestSurface();
			sharp.Canvas.FillColor = Colors.Black;
			sharp.Canvas.FillRectangle(30, 30, 40, 40);

			using var blurred = new CairoTestSurface();
			blurred.Canvas.SetBlur(10);
			blurred.Canvas.FillColor = Colors.Black;
			blurred.Canvas.FillRectangle(30, 30, 40, 40);

			Assert.Equal(255, sharp.Alpha(29, 50) + 255);
			Assert.True(sharp.IsTransparent(25, 50));
			Assert.InRange(blurred.Alpha(25, 50), 1, 254);
			Assert.Equal(255, blurred.Alpha(50, 50));
		}

		[Fact]
		public void RotatedDrawingStillCastsAShadowInDeviceSpace()
		{
			using var s = new CairoTestSurface();
			s.Canvas.Rotate(90, 50, 50);
			s.Canvas.SetShadow(new SizeF(20, 0), 0, Colors.Black);
			s.Canvas.FillColor = Colors.Red;
			s.Canvas.FillRectangle(40, 10, 20, 20);

			// rotated by 90 degrees clockwise the square sits at x 70..90, y 40..60
			Assert.Equal((255, 255, 0, 0), s.Pixel(80, 50));
			// the shadow is offset 20 device pixels to the right whatever the rotation
			Assert.False(s.IsTransparent(95, 50) && s.IsTransparent(99, 50));
		}

		[Fact]
		public void UnattachedCanvasThrowsAClearError()
		{
			using var canvas = new CairoCanvas();
			Assert.Throws<System.InvalidOperationException>(() => canvas.FillRectangle(0, 0, 10, 10));
		}

		[Fact]
		public void SettingTheContextResetsTheState()
		{
			using var a = new CairoTestSurface();
			using var b = new CairoTestSurface();
			using var canvas = new CairoCanvas(a.Context);
			canvas.FillColor = Colors.Red;
			canvas.Translate(50, 50);

			canvas.Context = b.Context;
			canvas.FillRectangle(0, 0, 10, 10);
			Assert.Null(a.PaintedBounds());
			Assert.Equal((255, 255, 255, 255), b.Pixel(5, 5));
		}

		private static PaintGradientStop[] Stops() =>
			new[] { new PaintGradientStop(0, Colors.Red), new PaintGradientStop(1, Colors.Blue) };

		private static LinearGradientPaint RedToBlue() =>
			new LinearGradientPaint(Stops(), new Point(0, 0), new Point(1, 0));

		private sealed class TestPattern : IPattern
		{
			public float Width => 10;

			public float Height => 10;

			public float StepX => 10;

			public float StepY => 10;

			public void Draw(ICanvas canvas)
			{
				canvas.FillColor = Colors.Red;
				canvas.FillRectangle(0, 0, 5, 5);
			}
		}
	}
}
