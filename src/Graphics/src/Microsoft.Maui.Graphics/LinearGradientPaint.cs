namespace Microsoft.Maui.Graphics
{
	public class LinearGradientPaint : GradientPaint
	{
		public LinearGradientPaint()
		{
		}

		public LinearGradientPaint(Point startPoint, Point endPoint)
		{
			StartPoint = startPoint;
			EndPoint = endPoint;
		}

		public Point StartPoint { get; }

		public Point EndPoint { get; }
	}
}
