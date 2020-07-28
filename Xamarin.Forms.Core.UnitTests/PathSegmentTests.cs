using NUnit.Framework;
using Xamarin.Forms.Shapes;

namespace Xamarin.Forms.Core.UnitTests
{
	public class PathSegmentTests : BaseTestFixture
	{
		[SetUp]
		public override void Setup()
		{
			base.Setup();

			Device.SetFlags(new[] { ExperimentalFlags.ShapesExperimental });
		}

		[Test]
		public void TestArcSegmentConstructor()
		{
			var arcSegment1 = new ArcSegment();
			Assert.IsNotNull(arcSegment1);

			var arcSegment2 = new ArcSegment(new Point(0, 0), new Size(100, 100), 90, SweepDirection.Clockwise, false);
			Assert.IsNotNull(arcSegment2);
			Assert.AreEqual(90, arcSegment2.RotationAngle);
			Assert.AreEqual(100, arcSegment2.Size.Height);
			Assert.AreEqual(100, arcSegment2.Size.Width);
		}

		[Test]
		public void TestBezierSegmentConstructor()
		{
			var bezierSegment1 = new BezierSegment();
			Assert.IsNotNull(bezierSegment1);

			var bezierSegment2 = new BezierSegment(new Point(0, 0), new Point(50, 50), new Point(100, 100));
			Assert.IsNotNull(bezierSegment2);
			Assert.AreEqual(0, bezierSegment2.Point1.X);
			Assert.AreEqual(0, bezierSegment2.Point1.Y);
			Assert.AreEqual(50, bezierSegment2.Point2.X);
			Assert.AreEqual(50, bezierSegment2.Point2.Y);
			Assert.AreEqual(100, bezierSegment2.Point3.X);
			Assert.AreEqual(100, bezierSegment2.Point3.Y);
		}

		[Test]
		public void TestLineSegmentConstructor()
		{
			var lineSegment1 = new LineSegment();
			Assert.IsNotNull(lineSegment1);

			var lineSegment2 = new LineSegment(new Point(25, 50));
			Assert.IsNotNull(lineSegment2);
			Assert.AreEqual(25, lineSegment2.Point.X);
			Assert.AreEqual(50, lineSegment2.Point.Y);
		}

		[Test]
		public void TestPolyBezierSegmentConstructor()
		{
			var polyBezierSegment1 = new PolyBezierSegment();
			Assert.IsNotNull(polyBezierSegment1);

			var polyBezierSegment2 = new PolyBezierSegment(new PointCollection { new Point(0, 0), new Point(1, 1) });
			Assert.IsNotNull(polyBezierSegment2);
			Assert.AreEqual(2, polyBezierSegment2.Points.Count);
		}

		[Test]
		public void TestPolyLineSegmentConstructor()
		{
			var polyLineSegment1 = new PolyLineSegment();
			Assert.IsNotNull(polyLineSegment1);

			var polyLineSegment2 = new PolyLineSegment(new PointCollection { new Point(0, 0), new Point(1, 1) });
			Assert.IsNotNull(polyLineSegment2);
			Assert.AreEqual(2, polyLineSegment2.Points.Count);
		}

		[Test]
		public void TestPolyQuadraticBezierSegmentConstructor()
		{
			var polyQuadraticBezierSegment1 = new PolyQuadraticBezierSegment();
			Assert.IsNotNull(polyQuadraticBezierSegment1);

			var polyQuadraticBezierSegment2 = new PolyQuadraticBezierSegment(new PointCollection { new Point(0, 0), new Point(1, 1) });
			Assert.IsNotNull(polyQuadraticBezierSegment2);
			Assert.AreEqual(2, polyQuadraticBezierSegment2.Points.Count);
		}

		[Test]
		public void TestQuadraticBezierSegmentConstructor()
		{
			var quadraticBezierSegment1 = new QuadraticBezierSegment();
			Assert.IsNotNull(quadraticBezierSegment1);

			var quadraticBezierSegment2 = new QuadraticBezierSegment(new Point(0, 0), new Point(100, 100));
			Assert.IsNotNull(quadraticBezierSegment2);
			Assert.AreEqual(0, quadraticBezierSegment2.Point1.X);
			Assert.AreEqual(0, quadraticBezierSegment2.Point1.Y);
			Assert.AreEqual(100, quadraticBezierSegment2.Point2.X);
			Assert.AreEqual(100, quadraticBezierSegment2.Point2.Y);
		}
	}
}