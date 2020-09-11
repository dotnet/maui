using System.Collections.Generic;
using NUnit.Framework;
using Xamarin.Forms.Shapes;

namespace Xamarin.Forms.Core.UnitTests
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
	}
}
