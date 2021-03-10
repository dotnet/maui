using System.Collections.Generic;
using Microsoft.Maui.Controls.Shapes;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	[TestFixture]
	public class GeometryTests : BaseTestFixture
	{
		[TestCase(0, true)]
		[TestCase(0, false)]
		[TestCase(45, true)]
		[TestCase(45, false)]
		[TestCase(180, true)]
		[TestCase(180, false)]
		[TestCase(270, true)]
		[TestCase(270, false)]
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

			Assert.AreNotEqual(0, points.Count);
		}

		[Test]
		public void TestRoundLineGeometryConstruction()
		{
			var lineGeometry = new LineGeometry(new Point(0, 0), new Point(100, 100));

			Assert.IsNotNull(lineGeometry);
			Assert.AreEqual(0, lineGeometry.StartPoint.X);
			Assert.AreEqual(0, lineGeometry.StartPoint.Y);
			Assert.AreEqual(100, lineGeometry.EndPoint.X);
			Assert.AreEqual(100, lineGeometry.EndPoint.Y);
		}

		[Test]
		public void TestEllipseGeometryConstruction()
		{
			var ellipseGeometry = new EllipseGeometry(new Point(50, 50), 10, 20);

			Assert.IsNotNull(ellipseGeometry);
			Assert.AreEqual(50, ellipseGeometry.Center.X);
			Assert.AreEqual(50, ellipseGeometry.Center.Y);
			Assert.AreEqual(10, ellipseGeometry.RadiusX);
			Assert.AreEqual(20, ellipseGeometry.RadiusY);
		}

		[Test]
		public void TestRectangleGeometryConstruction()
		{
			var rectangleGeometry = new RectangleGeometry(new Rect(0, 0, 150, 150));

			Assert.IsNotNull(rectangleGeometry);
			Assert.AreEqual(150, rectangleGeometry.Rect.Height);
			Assert.AreEqual(150, rectangleGeometry.Rect.Width);
		}

		[Test]
		public void TestRoundRectangleGeometryConstruction()
		{
			var roundRectangleGeometry = new RoundRectangleGeometry(new CornerRadius(12, 0, 0, 12), new Rect(0, 0, 150, 150));

			Assert.IsNotNull(roundRectangleGeometry);
			Assert.AreEqual(12, roundRectangleGeometry.CornerRadius.TopLeft);
			Assert.AreEqual(0, roundRectangleGeometry.CornerRadius.TopRight);
			Assert.AreEqual(0, roundRectangleGeometry.CornerRadius.BottomLeft);
			Assert.AreEqual(12, roundRectangleGeometry.CornerRadius.BottomRight);
			Assert.AreEqual(150, roundRectangleGeometry.Rect.Height);
			Assert.AreEqual(150, roundRectangleGeometry.Rect.Width);
		}
	}
}
