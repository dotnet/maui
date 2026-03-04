namespace Microsoft.Maui.Graphics
{
	/// <summary>
	/// Represents a radial gradient paint that transitions colors outward from a center point.
	/// </summary>
	public class RadialGradientPaint : GradientPaint
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="RadialGradientPaint"/> class with default center (0.5,0.5) and radius 0.5.
		/// </summary>
		public RadialGradientPaint()
		{
			Center = new Point(0.5, 0.5);
			Radius = 0.5;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="RadialGradientPaint"/> class with properties from another gradient paint.
		/// </summary>
		/// <param name="gradientPaint">The gradient paint to copy properties from.</param>
		public RadialGradientPaint(GradientPaint gradientPaint) : base(gradientPaint)
		{

		}

		/// <summary>
		/// Initializes a new instance of the <see cref="RadialGradientPaint"/> class with the specified gradient stops.
		/// </summary>
		/// <param name="gradientStops">The color stops for the gradient.</param>
		public RadialGradientPaint(PaintGradientStop[] gradientStops)
		{
			GradientStops = gradientStops;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="RadialGradientPaint"/> class with the specified center point and radius.
		/// </summary>
		/// <param name="center">The center point of the radial gradient.</param>
		/// <param name="radius">The radius of the radial gradient.</param>
		public RadialGradientPaint(Point center, double radius)
		{
			Center = center;
			Radius = radius;
		}

		public RadialGradientPaint(PaintGradientStop[] gradientStops, Point center, double radius)
		{
			GradientStops = gradientStops;
			Center = center;
			Radius = radius;
		}

		public Point Center { get; set; }

		public double Radius { get; set; }
	}
}