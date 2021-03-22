namespace Microsoft.Maui.Graphics
{
	public class LineGeometry : Geometry
	{
		public LineGeometry()
		{

		}

		public LineGeometry(Point startPoint, Point endPoint)
		{
			StartPoint = startPoint;
			EndPoint = endPoint;
		}

		public Point StartPoint { get; set; }

		public Point EndPoint { get; set; }
	}
}