#nullable disable
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Shapes
{
	/// <summary>
	/// Represents a path segment that draws a quadratic Bezier curve.
	/// </summary>
	public class QuadraticBezierSegment : PathSegment
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="QuadraticBezierSegment"/> class.
		/// </summary>
		public QuadraticBezierSegment()
		{

		}

		/// <summary>
		/// Initializes a new instance of the <see cref="QuadraticBezierSegment"/> class with the specified control point and endpoint.
		/// </summary>
		/// <param name="point1">The control point of the curve.</param>
		/// <param name="point2">The endpoint of the curve.</param>
		public QuadraticBezierSegment(Point point1, Point point2)
		{
			Point1 = point1;
			Point2 = point2;
		}

		/// <summary>Bindable property for <see cref="Point1"/>.</summary>
		public static readonly BindableProperty Point1Property =
			BindableProperty.Create(nameof(Point1), typeof(Point), typeof(QuadraticBezierSegment), new Point(0, 0));

		/// <summary>Bindable property for <see cref="Point2"/>.</summary>
		public static readonly BindableProperty Point2Property =
			BindableProperty.Create(nameof(Point2), typeof(Point), typeof(QuadraticBezierSegment), new Point(0, 0));

		/// <summary>
		/// Gets or sets the control point of the quadratic Bezier curve. This is a bindable property.
		/// </summary>
		public Point Point1
		{
			set { SetValue(Point1Property, value); }
			get { return (Point)GetValue(Point1Property); }
		}

		/// <summary>
		/// Gets or sets the endpoint of the quadratic Bezier curve. This is a bindable property.
		/// </summary>
		public Point Point2
		{
			set { SetValue(Point2Property, value); }
			get { return (Point)GetValue(Point2Property); }
		}
	}
}