namespace Microsoft.Maui.Graphics
{
	public class RadialGradientBrush : GradientBrush
	{
		public RadialGradientBrush()
		{

		}

		public RadialGradientBrush(GradientStopCollection gradientStops)
		{
			GradientStops = gradientStops;
		}

		public RadialGradientBrush(GradientStopCollection gradientStops, double radius)
		{
			GradientStops = gradientStops;
			Radius = radius;
		}

		public RadialGradientBrush(GradientStopCollection gradientStops, Point center, double radius)
		{
			GradientStops = gradientStops;
			Center = center;
			Radius = radius;
		}

		public override bool IsEmpty
		{
			get
			{
				var radialGradientBrush = this;
				return radialGradientBrush == null || radialGradientBrush.GradientStops.Count == 0;
			}
		}

		public Point Center { get; set; } = new Point(0.5, 0.5);

		public double Radius { get; set; } = 0.5d;
	}
}