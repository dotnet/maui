namespace Microsoft.Maui.Graphics
{
	public class LinearGradientPaint : GradientPaint
	{
		public LinearGradientPaint()
		{
			StartPoint = new Point(0, 0);
			EndPoint = new Point(1, 1);
		}

		public LinearGradientPaint(Point startPoint, Point endPoint)
		{
			StartPoint = startPoint;
			EndPoint = endPoint;
		}

		public Point StartPoint { get; set; }

		public Point EndPoint { get; set; }
	}
}
