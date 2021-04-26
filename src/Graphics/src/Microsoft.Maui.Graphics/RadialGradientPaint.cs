namespace Microsoft.Maui.Graphics
{
	public class RadialGradientPaint : GradientPaint
	{
		public RadialGradientPaint()
		{
			Center = new Point(0.5, 0.5);
			Radius = 0.5;
		}

		public RadialGradientPaint(Point center, double radius)
		{
			Center = center;
			Radius = radius;
		}

		public Point Center { get; set; }

		public double Radius { get; set; }
	}
}
