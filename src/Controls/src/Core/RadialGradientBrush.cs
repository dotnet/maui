using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
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

		public static readonly BindableProperty CenterProperty = BindableProperty.Create(
			nameof(Center), typeof(Point), typeof(RadialGradientBrush), new Point(0.5, 0.5));

		public Point Center
		{
			get => (Point)GetValue(CenterProperty);
			set => SetValue(CenterProperty, value);
		}

		public static readonly BindableProperty RadiusProperty = BindableProperty.Create(
			nameof(Radius), typeof(double), typeof(RadialGradientBrush), 0.5d);

		public double Radius
		{
			get => (double)GetValue(RadiusProperty);
			set => SetValue(RadiusProperty, value);
		}
	}
}