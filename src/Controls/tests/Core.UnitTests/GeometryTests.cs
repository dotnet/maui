using System.Collections.Generic;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{

	public class GeometryTests : BaseTestFixture
	{
		[Theory]
		[InlineData(0, true)]
		[InlineData(0, false)]
		[InlineData(45, true)]
		[InlineData(45, false)]
		[InlineData(180, true)]
		[InlineData(180, false)]
		[InlineData(270, true)]
		[InlineData(270, false)]
		public void FlattenArcTest(double angle, bool isLargeArc)
		{
			var path = new Path
			{
				HeightRequest = 200,
				WidthRequest = 200,
				Stroke = Brush.Black
			};

			PathFigure figure = new PathFigure();

			ArcSegment arcSegment = new ArcSegment
			{
				Point = new Point(10, 100),
				Size = new Size(100, 50),
				RotationAngle = angle,
				IsLargeArc = isLargeArc
			};

			figure.Segments.Add(arcSegment);

			path.Data = new PathGeometry
			{
				Figures = new PathFigureCollection
				{
					figure
				}
			};

			List<Point> points = new List<Point>();

			GeometryHelper.FlattenArc(
				points,
				Point.Zero,
				arcSegment.Point,
				arcSegment.Size.Width,
				arcSegment.Size.Height,
				arcSegment.RotationAngle,
				arcSegment.IsLargeArc,
				arcSegment.SweepDirection == SweepDirection.CounterClockwise,
				1);

			Assert.NotEmpty(points);
		}

		[Fact]
		public void FlattenSmallArcProducesFinitePoints()
		{
			var points = new List<Point>();

			GeometryHelper.FlattenArc(
				points,
				Point.Zero,
				new Point(-0.55999, -0.44),
				1,
				1,
				0,
				false,
				false,
				1);

			Assert.Equal(2, points.Count);
			Assert.All(points, point =>
			{
				Assert.True(double.IsFinite(point.X));
				Assert.True(double.IsFinite(point.Y));
			});
		}

		[Fact]
		public void FlattenSmallCubicBezierProducesFinitePoints()
		{
			var points = new List<Point>();

			GeometryHelper.FlattenCubicBezier(
				points,
				Point.Zero,
				new Point(0.1, 0.1),
				new Point(0.2, 0.1),
				new Point(0.3, 0),
				1);

			Assert.Equal(2, points.Count);
			Assert.All(points, point =>
			{
				Assert.True(double.IsFinite(point.X));
				Assert.True(double.IsFinite(point.Y));
			});
		}

		[Fact]
		public void FlattenSmallQuadraticBezierProducesFinitePoints()
		{
			var points = new List<Point>();

			GeometryHelper.FlattenQuadraticBezier(
				points,
				Point.Zero,
				new Point(0.2, 0.2),
				new Point(0.4, 0),
				1);

			Assert.Equal(2, points.Count);
			Assert.All(points, point =>
			{
				Assert.True(double.IsFinite(point.X));
				Assert.True(double.IsFinite(point.Y));
			});
		}

		[Fact]
		public void TestRoundLineGeometryConstruction()
		{
			var lineGeometry = new LineGeometry(new Point(0, 0), new Point(100, 100));

			Assert.NotNull(lineGeometry);
			Assert.Equal(0, lineGeometry.StartPoint.X);
			Assert.Equal(0, lineGeometry.StartPoint.Y);
			Assert.Equal(100, lineGeometry.EndPoint.X);
			Assert.Equal(100, lineGeometry.EndPoint.Y);
		}

		[Theory]
		[InlineData(15, 15, 19, 19)]
		[InlineData(0, 0, 100, 100)]
		[InlineData(-10.5, -20.25, 30.75, 40.5)]
		[InlineData(19, 19, 15, 15)]
		[InlineData(15, 15, 15, 15)]
		public void LineGeometryStartsAtStartPoint(double startX, double startY, double endX, double endY)
		{
			var geometry = new LineGeometry(new Point(startX, startY), new Point(endX, endY));
			var path = ((IShape)geometry).PathForBounds(new Rect(0, 0, 100, 100));

			Assert.Equal(new[] { PathOperation.Move, PathOperation.Line }, path.SegmentTypes);
			Assert.Equal(new[] { new PointF((float)startX, (float)startY), new PointF((float)endX, (float)endY) }, path.Points);
			Assert.Equal(1, path.SubPathCount);
			Assert.False(path.Closed);
		}

		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public void LineGeometryAppendPreservesExistingFigure(bool closed)
		{
			var path = new PathF();
			path.MoveTo(1, 2);
			path.LineTo(3, 4);
			if (closed)
				path.Close();

			var expectedOperations = new List<PathOperation>(path.SegmentTypes)
			{
				PathOperation.Move,
				PathOperation.Line
			};
			var geometry = new LineGeometry(new Point(15, 16), new Point(19, 20));

			geometry.AppendPath(path);

			Assert.Equal(expectedOperations, path.SegmentTypes);
			Assert.Equal(new[] { new PointF(1, 2), new PointF(3, 4), new PointF(15, 16), new PointF(19, 20) }, path.Points);
			Assert.Equal(2, path.SubPathCount);
			Assert.Equal(closed, path.IsSubPathClosed(0));
			Assert.False(path.IsSubPathClosed(1));
		}

		[Theory]
		[InlineData(FillRule.Nonzero, false)]
		[InlineData(FillRule.Nonzero, true)]
		[InlineData(FillRule.EvenOdd, false)]
		[InlineData(FillRule.EvenOdd, true)]
		public void GeometryGroupPreservesEllipseAndLine(FillRule fillRule, bool lineFirst)
		{
			var ellipse = new EllipseGeometry(new Point(10, 10), 7, 7);
			var line = new LineGeometry(new Point(15, 15), new Point(19, 19));
			var group = new GeometryGroup { FillRule = fillRule };
			if (lineFirst)
				group.Children.Add(line);
			group.Children.Add(ellipse);
			if (!lineFirst)
				group.Children.Add(line);

			var expectedPath = new PathF();
			if (lineFirst)
				expectedPath.MoveTo(15, 15).LineTo(19, 19);
			ellipse.AppendPath(expectedPath);
			if (!lineFirst)
				expectedPath.MoveTo(15, 15).LineTo(19, 19);

			var path = new Path(group).GetPath();

			Assert.Equal(expectedPath.SegmentTypes, path.SegmentTypes);
			Assert.Equal(expectedPath.Points, path.Points);
			Assert.Equal(2, path.SubPathCount);
			Assert.True(path.IsSubPathClosed(lineFirst ? 1 : 0));
			Assert.False(path.IsSubPathClosed(lineFirst ? 0 : 1));
		}

		[Fact]
		public void NestedGeometryGroupKeepsLinesInSeparateFigures()
		{
			var nestedGroup = new GeometryGroup();
			nestedGroup.Children.Add(new LineGeometry(new Point(1, 2), new Point(3, 4)));
			var group = new GeometryGroup();
			group.Children.Add(nestedGroup);
			group.Children.Add(new LineGeometry(new Point(15, 16), new Point(19, 20)));

			var path = new Path(group).GetPath();

			Assert.Equal(new[] { PathOperation.Move, PathOperation.Line, PathOperation.Move, PathOperation.Line }, path.SegmentTypes);
			Assert.Equal(new[] { new PointF(1, 2), new PointF(3, 4), new PointF(15, 16), new PointF(19, 20) }, path.Points);
			Assert.Equal(2, path.SubPathCount);
			Assert.False(path.IsSubPathClosed(0));
			Assert.False(path.IsSubPathClosed(1));
		}

		[Fact]
		public void TestEllipseGeometryConstruction()
		{
			var ellipseGeometry = new EllipseGeometry(new Point(50, 50), 10, 20);

			Assert.NotNull(ellipseGeometry);
			Assert.Equal(50, ellipseGeometry.Center.X);
			Assert.Equal(50, ellipseGeometry.Center.Y);
			Assert.Equal(10, ellipseGeometry.RadiusX);
			Assert.Equal(20, ellipseGeometry.RadiusY);
		}

		[Fact]
		public void TestRectangleGeometryConstruction()
		{
			var rectangleGeometry = new RectangleGeometry(new Rect(0, 0, 150, 150));

			Assert.NotNull(rectangleGeometry);
			Assert.Equal(150, rectangleGeometry.Rect.Height);
			Assert.Equal(150, rectangleGeometry.Rect.Width);
		}

		[Fact]
		public void TestRoundRectangleGeometryConstruction()
		{
			var roundRectangleGeometry = new RoundRectangleGeometry(new CornerRadius(12, 0, 0, 12), new Rect(0, 0, 150, 150));

			Assert.NotNull(roundRectangleGeometry);
			Assert.Equal(12, roundRectangleGeometry.CornerRadius.TopLeft);
			Assert.Equal(0, roundRectangleGeometry.CornerRadius.TopRight);
			Assert.Equal(0, roundRectangleGeometry.CornerRadius.BottomLeft);
			Assert.Equal(12, roundRectangleGeometry.CornerRadius.BottomRight);
			Assert.Equal(150, roundRectangleGeometry.Rect.Height);
			Assert.Equal(150, roundRectangleGeometry.Rect.Width);
		}
	}
}
