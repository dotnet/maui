namespace Microsoft.Maui.Graphics
{
	public class BezierSegment : PathSegment
	{
		public BezierSegment()
		{

		}

		public BezierSegment(Point point1, Point point2, Point point3)
		{
			Point1 = point1;
			Point2 = point2;
			Point3 = point3;
		}

		public Point Point1 { get; set; }

		public Point Point2 { get; set; }

		public Point Point3 { get; set; }
	}
}