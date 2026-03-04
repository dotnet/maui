#nullable disable
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Shapes
{
	/// <summary>
	/// Represents a path segment that draws a cubic Bezier curve defined by three points.
	/// </summary>
	public class BezierSegment : PathSegment
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="BezierSegment"/> class with default control points.
		/// </summary>
		public BezierSegment()
		{

		}

		/// <summary>
		/// Initializes a new instance of the <see cref="BezierSegment"/> class with the specified control and end points.
		/// </summary>
		/// <param name="point1">The first control point of the Bezier curve.</param>
		/// <param name="point2">The second control point of the Bezier curve.</param>
		/// <param name="point3">The end point of the Bezier curve.</param>
		public BezierSegment(Point point1, Point point2, Point point3)
		{
			Point1 = point1;
			Point2 = point2;
			Point3 = point3;
		}

		/// <summary>Bindable property for <see cref="Point1"/>.</summary>
		public static readonly BindableProperty Point1Property =
			BindableProperty.Create(nameof(Point1), typeof(Point), typeof(BezierSegment), new Point(0, 0));

		/// <summary>Bindable property for <see cref="Point2"/>.</summary>
		public static readonly BindableProperty Point2Property =
			BindableProperty.Create(nameof(Point2), typeof(Point), typeof(BezierSegment), new Point(0, 0));

		/// <summary>Bindable property for <see cref="Point3"/>.</summary>
		public static readonly BindableProperty Point3Property =
			BindableProperty.Create(nameof(Point3), typeof(Point), typeof(BezierSegment), new Point(0, 0));

		/// <summary>
		/// Gets or sets the first control point of the cubic Bezier curve. This is a bindable property.
		/// </summary>
		public Point Point1
		{
			set { SetValue(Point1Property, value); }
			get { return (Point)GetValue(Point1Property); }
		}

		/// <summary>
		/// Gets or sets the second control point of the cubic Bezier curve. This is a bindable property.
		/// </summary>
		public Point Point2
		{
			set { SetValue(Point2Property, value); }
			get { return (Point)GetValue(Point2Property); }
		}

		/// <summary>
		/// Gets or sets the end point of the cubic Bezier curve. This is a bindable property.
		/// </summary>
		public Point Point3
		{
			set { SetValue(Point3Property, value); }
			get { return (Point)GetValue(Point3Property); }
		}
	}
}