namespace Microsoft.Maui.Graphics
{
	public class QuadraticBezierSegment : PathSegment
	{
		public QuadraticBezierSegment()
		{

		}

		public QuadraticBezierSegment(Point point1, Point point2)
		{
			Point1 = point1;
			Point2 = point2;
		}

		public Point Point1 { get; set; } = new Point(0, 0);

		public Point Point2 { get; set; } = new Point(0, 0);
	}
}