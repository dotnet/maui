namespace Microsoft.Maui.Graphics
{
	public class RadialGradientPaint : GradientPaint
	{
		public RadialGradientPaint()
		{
			Center = new Point(0.5, 0.5);
			Radius = 0.5;
		}

		public RadialGradientPaint(GradientPaint gradientPaint) : base(gradientPaint)
		{

		}

		public RadialGradientPaint(GradientStop[] gradientStops)
		{
			GradientStops = gradientStops;
		}

		public RadialGradientPaint(Point center, double radius)
		{
			Center = center;
			Radius = radius;
		}

		public RadialGradientPaint(GradientStop[] gradientStops, Point center, double radius)
		{
			GradientStops = gradientStops;
			Center = center;
			Radius = radius;
		}

		public Point Center { get; set; }

		public double Radius { get; set; }
	}
}