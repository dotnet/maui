#nullable disable
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	/// <summary>A <see cref="GradientBrush"/> that paints an area with a radial gradient.</summary>
	public class RadialGradientBrush : GradientBrush
	{
		/// <summary>Initializes a new instance of the <see cref="RadialGradientBrush"/> class.</summary>
		public RadialGradientBrush()
		{

		}

		/// <summary>Initializes a new instance of the <see cref="RadialGradientBrush"/> class with the specified gradient stops.</summary>
		/// <param name="gradientStops">The collection of gradient stops.</param>
		public RadialGradientBrush(GradientStopCollection gradientStops)
		{
			GradientStops = gradientStops;
		}

		/// <summary>Initializes a new instance of the <see cref="RadialGradientBrush"/> class with the specified gradient stops and radius.</summary>
		/// <param name="gradientStops">The collection of gradient stops.</param>
		/// <param name="radius">The radius of the gradient.</param>
		public RadialGradientBrush(GradientStopCollection gradientStops, double radius)
		{
			GradientStops = gradientStops;
			Radius = radius;
		}

		/// <summary>Initializes a new instance of the <see cref="RadialGradientBrush"/> class with the specified gradient stops, center, and radius.</summary>
		/// <param name="gradientStops">The collection of gradient stops.</param>
		/// <param name="center">The center point of the gradient.</param>
		/// <param name="radius">The radius of the gradient.</param>
		public RadialGradientBrush(GradientStopCollection gradientStops, Point center, double radius)
		{
			GradientStops = gradientStops;
			Center = center;
			Radius = radius;
		}

		public override bool IsEmpty => base.IsEmpty;

		/// <summary>Bindable property for <see cref="Center"/>.</summary>
		public static readonly BindableProperty CenterProperty = BindableProperty.Create(
			nameof(Center), typeof(Point), typeof(RadialGradientBrush), new Point(0.5, 0.5));

		/// <summary>Gets or sets the center point of the gradient. This is a bindable property.</summary>
		public Point Center
		{
			get => (Point)GetValue(CenterProperty);
			set => SetValue(CenterProperty, value);
		}

		/// <summary>Bindable property for <see cref="Radius"/>.</summary>
		public static readonly BindableProperty RadiusProperty = BindableProperty.Create(
			nameof(Radius), typeof(double), typeof(RadialGradientBrush), 0.5d);

		/// <summary>Gets or sets the radius of the gradient. This is a bindable property.</summary>
		public double Radius
		{
			get => (double)GetValue(RadiusProperty);
			set => SetValue(RadiusProperty, value);
		}
	}
}