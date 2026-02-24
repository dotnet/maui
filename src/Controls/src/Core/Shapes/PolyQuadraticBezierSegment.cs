#nullable disable
namespace Microsoft.Maui.Controls.Shapes
{
	/// <summary>
	/// A path segment that defines one or more connected quadratic Bezier curves.
	/// </summary>
	public class PolyQuadraticBezierSegment : PathSegment
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="PolyQuadraticBezierSegment"/> class with an empty point collection.
		/// </summary>
		public PolyQuadraticBezierSegment()
		{
			Points = new PointCollection();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="PolyQuadraticBezierSegment"/> class with the specified points.
		/// </summary>
		/// <param name="points">The collection of control and end points defining the quadratic Bezier curves.</param>
		public PolyQuadraticBezierSegment(PointCollection points)
		{
			Points = points;
		}

		/// <summary>Bindable property for <see cref="Points"/>.</summary>
		public static readonly BindableProperty PointsProperty =
			BindableProperty.Create(nameof(Points), typeof(PointCollection), typeof(PolyQuadraticBezierSegment), null);

		/// <summary>
		/// Gets or sets the control and end points of the quadratic Bezier curves. This is a bindable property.
		/// </summary>
		public PointCollection Points
		{
			set { SetValue(PointsProperty, value); }
			get { return (PointCollection)GetValue(PointsProperty); }
		}
	}
}