using System.Numerics;
using Xunit;

namespace Microsoft.Maui.Graphics.Cairo.Tests
{
	public class ShapeTests
	{
		[Fact]
		public void FillRectangleFillsOnlyTheRectangle()
		{
			using var s = new CairoTestSurface();
			s.Canvas.FillColor = Colors.Red;
			s.Canvas.FillRectangle(10, 20, 30, 40);

			Assert.Equal((255, 255, 0, 0), s.Pixel(25, 40));
			Assert.True(s.IsTransparent(5, 40));
			Assert.True(s.IsTransparent(25, 15));
			Assert.True(s.IsTransparent(45, 40));
			Assert.True(s.IsTransparent(25, 65));
			Assert.Equal((10, 20, 40, 60), s.PaintedBounds());
		}

		[Fact]
		public void DrawRectangleStrokesOnlyTheOutline()
		{
			using var s = new CairoTestSurface();
			s.Canvas.StrokeColor = Colors.Blue;
			s.Canvas.StrokeSize = 4;
			s.Canvas.DrawRectangle(20, 20, 40, 40);

			Assert.Equal((255, 0, 0, 255), s.Pixel(20, 40));
			Assert.Equal((255, 0, 0, 255), s.Pixel(40, 20));
			Assert.True(s.IsTransparent(40, 40));
			Assert.True(s.IsTransparent(10, 40));
		}

		[Fact]
		public void DrawLineUsesStrokeSizeAndColor()
		{
			using var s = new CairoTestSurface();
			s.Canvas.StrokeColor = Colors.Green;
			s.Canvas.StrokeSize = 6;
			s.Canvas.DrawLine(10, 50, 90, 50);

			Assert.InRange((int)s.Pixel(50, 48).G, 126, 130);
			Assert.True(s.IsTransparent(50, 44));
			Assert.True(s.IsTransparent(50, 56));
			Assert.True(s.IsTransparent(5, 50));
		}

		[Fact]
		public void ZeroStrokeSizeDrawsNothing()
		{
			using var s = new CairoTestSurface();
			s.Canvas.StrokeSize = 0;
			s.Canvas.DrawRectangle(10, 10, 50, 50);
			Assert.Null(s.PaintedBounds());
		}

		[Fact]
		public void StrokeLineCapChangesTheLineExtent()
		{
			using var butt = new CairoTestSurface();
			butt.Canvas.StrokeSize = 10;
			butt.Canvas.StrokeLineCap = LineCap.Butt;
			butt.Canvas.DrawLine(30, 50, 70, 50);
			Assert.True(butt.IsTransparent(27, 50));

			using var square = new CairoTestSurface();
			square.Canvas.StrokeSize = 10;
			square.Canvas.StrokeLineCap = LineCap.Square;
			square.Canvas.DrawLine(30, 50, 70, 50);
			Assert.False(square.IsTransparent(27, 50));
			Assert.True(square.IsTransparent(23, 50));

			using var round = new CairoTestSurface();
			round.Canvas.StrokeSize = 10;
			round.Canvas.StrokeLineCap = LineCap.Round;
			round.Canvas.DrawLine(30, 50, 70, 50);
			Assert.False(round.IsTransparent(27, 50));
			// the round cap does not reach the corner a square cap covers
			Assert.True(round.IsTransparent(25, 45));
		}

		[Fact]
		public void StrokeLineJoinChangesTheCorner()
		{
			var path = new PathF();
			path.MoveTo(20, 80);
			path.LineTo(20, 20);
			path.LineTo(80, 20);

			using var miter = new CairoTestSurface();
			miter.Canvas.StrokeSize = 10;
			miter.Canvas.StrokeLineJoin = LineJoin.Miter;
			miter.Canvas.DrawPath(path);
			Assert.False(miter.IsTransparent(16, 16));

			using var bevel = new CairoTestSurface();
			bevel.Canvas.StrokeSize = 10;
			bevel.Canvas.StrokeLineJoin = LineJoin.Bevel;
			bevel.Canvas.DrawPath(path);
			Assert.True(bevel.IsTransparent(16, 16));
		}

		[Fact]
		public void DashPatternIsInMultiplesOfTheStrokeSize()
		{
			using var s = new CairoTestSurface(120, 40);
			s.Canvas.StrokeSize = 4;
			s.Canvas.StrokeDashPattern = new float[] { 2, 1 };
			s.Canvas.StrokeDashOffset = 0;
			s.Canvas.DrawLine(0, 20, 120, 20);

			// dash 0..8, gap 8..12, dash 12..20, gap 20..24
			Assert.False(s.IsTransparent(4, 20));
			Assert.True(s.IsTransparent(10, 20));
			Assert.False(s.IsTransparent(16, 20));
			Assert.True(s.IsTransparent(22, 20));
		}

		[Fact]
		public void FillEllipseFillsTheInscribedEllipse()
		{
			using var s = new CairoTestSurface();
			s.Canvas.FillColor = Colors.Red;
			s.Canvas.FillEllipse(10, 10, 80, 60);

			Assert.False(s.IsTransparent(50, 40));
			Assert.False(s.IsTransparent(12, 40));
			Assert.True(s.IsTransparent(12, 12));
			Assert.True(s.IsTransparent(88, 68));
			Assert.Equal((10, 10, 90, 70), s.PaintedBounds());
		}

		[Fact]
		public void DrawEllipseIsHollow()
		{
			using var s = new CairoTestSurface();
			s.Canvas.StrokeSize = 2;
			s.Canvas.DrawEllipse(10, 10, 80, 80);
			Assert.False(s.IsTransparent(10, 50));
			Assert.True(s.IsTransparent(50, 50));
		}

		[Fact]
		public void FillRoundedRectangleRoundsTheCorners()
		{
			using var s = new CairoTestSurface();
			s.Canvas.FillColor = Colors.Red;
			s.Canvas.FillRoundedRectangle(10, 10, 80, 80, 20);

			Assert.True(s.IsTransparent(11, 11));
			Assert.False(s.IsTransparent(30, 11));
			Assert.False(s.IsTransparent(11, 30));
			Assert.False(s.IsTransparent(50, 50));
		}

		[Fact]
		public void DrawRoundedRectangleClampsTheRadius()
		{
			using var s = new CairoTestSurface();
			s.Canvas.StrokeSize = 2;
			s.Canvas.DrawRoundedRectangle(10, 10, 80, 20, 500);
			// a pill: the left-most point is on the vertical middle, the corner is empty
			Assert.False(s.IsTransparent(10, 20));
			Assert.True(s.IsTransparent(11, 11));
		}

		[Fact]
		public void FillArcFillsTheCounterClockwiseSegment()
		{
			using var s = new CairoTestSurface();
			s.Canvas.FillColor = Colors.Red;
			// 0 -> 90 degrees counter-clockwise: the upper right quarter of the circle
			s.Canvas.FillArc(10, 10, 80, 80, 0, 90, false);

			Assert.False(s.IsTransparent(80, 25));
			Assert.True(s.IsTransparent(20, 80));
			Assert.True(s.IsTransparent(20, 20));
		}

		[Fact]
		public void FillArcClockwiseRunsTheOtherWay()
		{
			using var s = new CairoTestSurface();
			s.Canvas.FillColor = Colors.Red;
			// 90 -> 0 clockwise is the same upper right quarter
			s.Canvas.FillArc(10, 10, 80, 80, 90, 0, true);

			Assert.False(s.IsTransparent(80, 25));
			Assert.True(s.IsTransparent(30, 70));

			using var other = new CairoTestSurface();
			other.Canvas.FillColor = Colors.Red;
			// 0 -> 90 clockwise is the other three quarters
			other.Canvas.FillArc(10, 10, 80, 80, 0, 90, true);
			Assert.False(other.IsTransparent(30, 70));
			Assert.False(other.IsTransparent(30, 30));
			Assert.True(other.IsTransparent(80, 25));
		}

		[Fact]
		public void DrawArcClosedJoinsTheEnds()
		{
			using var open = new CairoTestSurface();
			open.Canvas.StrokeSize = 2;
			open.Canvas.DrawArc(10, 10, 80, 80, 0, 90, false, false);

			using var closed = new CairoTestSurface();
			closed.Canvas.StrokeSize = 2;
			closed.Canvas.DrawArc(10, 10, 80, 80, 0, 90, false, true);

			// the chord runs from (90, 50) to (50, 10); its middle is at (70, 30)
			Assert.True(open.IsTransparent(70, 30));
			Assert.False(closed.IsTransparent(70, 30));
		}

		[Fact]
		public void FillPathHonoursTheWindingMode()
		{
			var path = new PathF();
			path.AppendRectangle(10, 10, 80, 80);
			path.AppendRectangle(30, 30, 40, 40);

			using var evenOdd = new CairoTestSurface();
			evenOdd.Canvas.FillColor = Colors.Red;
			evenOdd.Canvas.FillPath(path, WindingMode.EvenOdd);
			Assert.True(evenOdd.IsTransparent(50, 50));
			Assert.False(evenOdd.IsTransparent(20, 20));

			using var nonZero = new CairoTestSurface();
			nonZero.Canvas.FillColor = Colors.Red;
			nonZero.Canvas.FillPath(path, WindingMode.NonZero);
			Assert.False(nonZero.IsTransparent(50, 50));
		}

		[Fact]
		public void CurvesAreFilled()
		{
			var path = new PathF();
			path.MoveTo(10, 90);
			path.QuadTo(50, -30, 90, 90);
			path.Close();

			using var quad = new CairoTestSurface();
			quad.Canvas.FillColor = Colors.Red;
			quad.Canvas.FillPath(path);
			// the quadratic peaks at y = 30 in the middle
			Assert.False(quad.IsTransparent(50, 40));
			Assert.True(quad.IsTransparent(50, 20));
			Assert.True(quad.IsTransparent(12, 20));

			var cubic = new PathF();
			cubic.MoveTo(10, 90);
			cubic.CurveTo(10, 0, 90, 0, 90, 90);
			cubic.Close();
			using var c = new CairoTestSurface();
			c.Canvas.FillColor = Colors.Red;
			c.Canvas.FillPath(cubic);
			Assert.False(c.IsTransparent(50, 50));
			Assert.True(c.IsTransparent(50, 10));
		}

		[Fact]
		public void PathArcSegmentsAreDrawn()
		{
			var path = new PathF();
			path.AddArc(10, 10, 90, 90, 0, 180, false);

			using var s = new CairoTestSurface();
			s.Canvas.FillColor = Colors.Red;
			s.Canvas.FillPath(path);
			// the upper half of the circle
			Assert.False(s.IsTransparent(50, 25));
			Assert.True(s.IsTransparent(50, 75));
		}

		[Fact]
		public void AntialiasCanBeSwitchedOff()
		{
			using var on = new CairoTestSurface();
			on.Canvas.FillColor = Colors.Black;
			on.Canvas.FillEllipse(10, 10, 60, 60);

			using var off = new CairoTestSurface();
			off.Canvas.Antialias = false;
			off.Canvas.FillColor = Colors.Black;
			off.Canvas.FillEllipse(10, 10, 60, 60);

			var partial = 0;
			var partialOff = 0;
			for (var y = 0; y < 100; y++)
			{
				for (var x = 0; x < 100; x++)
				{
					var a = on.Alpha(x, y);
					if (a != 0 && a != 255)
						partial++;
					a = off.Alpha(x, y);
					if (a != 0 && a != 255)
						partialOff++;
				}
			}

			Assert.True(partial > 0);
			Assert.Equal(0, partialOff);
		}

		[Fact]
		public void AlphaScalesTheColor()
		{
			using var s = new CairoTestSurface();
			s.Canvas.Alpha = 0.5f;
			s.Canvas.FillColor = Colors.Red;
			s.Canvas.FillRectangle(0, 0, 50, 50);
			Assert.InRange(s.Alpha(10, 10), 126, 130);

			s.Canvas.StrokeColor = Colors.Blue;
			s.Canvas.StrokeSize = 10;
			s.Canvas.DrawLine(0, 80, 100, 80);
			Assert.InRange(s.Alpha(50, 80), 126, 130);
		}

		[Fact]
		public void BlendModesUseCairoOperators()
		{
			using var s = new CairoTestSurface();
			s.Canvas.FillColor = Colors.White;
			s.Canvas.FillRectangle(0, 0, 100, 100);

			s.Canvas.BlendMode = BlendMode.Multiply;
			s.Canvas.FillColor = Color.FromRgb(128, 64, 255);
			s.Canvas.FillRectangle(0, 0, 50, 100);

			var multiplied = s.Pixel(25, 50);
			Assert.InRange((int)multiplied.R, 126, 130);
			Assert.InRange((int)multiplied.G, 62, 66);
			Assert.InRange((int)multiplied.B, 253, 255);

			s.Canvas.BlendMode = BlendMode.Clear;
			s.Canvas.FillRectangle(60, 0, 20, 100);
			Assert.True(s.IsTransparent(70, 50));
			Assert.Equal((255, 255, 255, 255), s.Pixel(90, 50));
		}

		[Fact]
		public void AsCairoOperatorMapsEveryBlendMode()
		{
			foreach (BlendMode mode in System.Enum.GetValues(typeof(BlendMode)))
				_ = mode.AsCairoOperator();

			Assert.Equal(global::Cairo.Operator.Over, BlendMode.Normal.AsCairoOperator());
			Assert.Equal(global::Cairo.Operator.Add, BlendMode.PlusLighter.AsCairoOperator());
			Assert.Equal((global::Cairo.Operator)14, BlendMode.Multiply.AsCairoOperator());
		}

		[Fact]
		public void ConcatenateTransformAppliesTheMatrix()
		{
			using var s = new CairoTestSurface();
			s.Canvas.FillColor = Colors.Red;
			s.Canvas.ConcatenateTransform(Matrix3x2.CreateScale(2, 2) * Matrix3x2.CreateTranslation(10, 0));
			s.Canvas.FillRectangle(0, 0, 10, 10);

			Assert.Equal((10, 0, 30, 20), s.PaintedBounds());
		}
	}
}
