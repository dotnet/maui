namespace Microsoft.Maui.Graphics
{
	public class RadialGradientPaint : GradientPaint
	{
		public RadialGradientPaint()
		{
		}

		public RadialGradientPaint(Point center, double radius)
		{
			Center = center;
			Radius = radius;
		}

		public Point Center { get; }

		public double Radius { get; }
	}
}
