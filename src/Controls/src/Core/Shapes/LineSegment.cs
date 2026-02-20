#nullable disable
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Shapes
{
	/// <summary>
	/// A path segment that draws a straight line to a specified point.
	/// </summary>
	public class LineSegment : PathSegment
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="LineSegment"/> class.
		/// </summary>
		public LineSegment()
		{

		}

		/// <summary>
		/// Initializes a new instance of the <see cref="LineSegment"/> class with the specified endpoint.
		/// </summary>
		/// <param name="point">The endpoint of the line segment.</param>
		public LineSegment(Point point)
		{
			Point = point;
		}

		/// <summary>Bindable property for <see cref="Point"/>.</summary>
		public static readonly BindableProperty PointProperty =
			BindableProperty.Create(nameof(Point), typeof(Point), typeof(LineSegment), new Point(0, 0));

		/// <summary>
		/// Gets or sets the endpoint of the line segment. This is a bindable property.
		/// </summary>
		public Point Point
		{
			set { SetValue(PointProperty, value); }
			get { return (Point)GetValue(PointProperty); }
		}
	}
}