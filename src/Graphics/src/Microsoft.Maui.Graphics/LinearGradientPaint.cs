namespace Microsoft.Maui.Graphics
{
	public class LinearGradientPaint : GradientPaint
	{
		public LinearGradientPaint()
		{
			StartPoint = new Point(0, 0);
			EndPoint = new Point(1, 1);
		}

		public LinearGradientPaint(GradientPaint gradientPaint) : base(gradientPaint)
		{
		
		}

		public LinearGradientPaint(PaintGradientStop[] gradientStops)
		{
			GradientStops = gradientStops;
		}

		public LinearGradientPaint(Point startPoint, Point endPoint)
		{
			StartPoint = startPoint;
			EndPoint = endPoint;
		}

		public LinearGradientPaint(PaintGradientStop[] gradientStops, Point startPoint, Point endPoint)
		{
			GradientStops = gradientStops;
			StartPoint = startPoint;
			EndPoint = endPoint;
		}

		public Point StartPoint { get; set; }

		public Point EndPoint { get; set; }
	}
}
