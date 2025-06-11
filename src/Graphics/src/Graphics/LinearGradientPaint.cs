namespace Microsoft.Maui.Graphics
{
	/// <summary>
	/// Represents a linear gradient paint that transitions colors along a line defined by start and end points.
	/// </summary>
	public class LinearGradientPaint : GradientPaint
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="LinearGradientPaint"/> class with default start point (0,0) and end point (1,1).
		/// </summary>
		public LinearGradientPaint()
		{
			StartPoint = new Point(0, 0);
			EndPoint = new Point(1, 1);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="LinearGradientPaint"/> class with properties from another gradient paint.
		/// </summary>
		/// <param name="gradientPaint">The gradient paint to copy properties from.</param>
		public LinearGradientPaint(GradientPaint gradientPaint) : base(gradientPaint)
		{

		}

		/// <summary>
		/// Initializes a new instance of the <see cref="LinearGradientPaint"/> class with the specified gradient stops.
		/// </summary>
		/// <param name="gradientStops">The array of gradient stops that define color transitions.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when <paramref name="gradientStops"/> is null.</exception>
		public LinearGradientPaint(PaintGradientStop[] gradientStops)
		{
			GradientStops = gradientStops;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="LinearGradientPaint"/> class with the specified start and end points.
		/// </summary>
		/// <param name="startPoint">The start point of the gradient.</param>
		/// <param name="endPoint">The end point of the gradient.</param>
		public LinearGradientPaint(Point startPoint, Point endPoint)
		{
			StartPoint = startPoint;
			EndPoint = endPoint;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="LinearGradientPaint"/> class with the specified gradient stops, start point, and end point.
		/// </summary>
		/// <param name="gradientStops">The array of gradient stops that define color transitions.</param>
		/// <param name="startPoint">The start point of the gradient.</param>
		/// <param name="endPoint">The end point of the gradient.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when <paramref name="gradientStops"/> is null.</exception>
		public LinearGradientPaint(PaintGradientStop[] gradientStops, Point startPoint, Point endPoint)
		{
			GradientStops = gradientStops;
			StartPoint = startPoint;
			EndPoint = endPoint;
		}

		/// <summary>
		/// Gets or sets the start point of the gradient.
		/// </summary>
		/// <remarks>
		/// The start point is typically expressed in relative coordinates from (0,0) to (1,1).
		/// </remarks>
		public Point StartPoint { get; set; }

		/// <summary>
		/// Gets or sets the end point of the gradient.
		/// </summary>
		/// <remarks>
		/// The end point is typically expressed in relative coordinates from (0,0) to (1,1).
		/// </remarks>
		public Point EndPoint { get; set; }
	}
}
