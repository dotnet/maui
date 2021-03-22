namespace Microsoft.Maui.Graphics
{
	public class LineSegment : PathSegment
	{
		public LineSegment()
		{

		}

		public LineSegment(Point point)
		{
			Point = point;
		}

		public Point Point { get; set; } = new Point(0, 0);
	}
}