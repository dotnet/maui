#nullable disable
namespace Microsoft.Maui.Controls.Shapes
{
	/// <summary>
	/// A path segment that draws one or more connected cubic Bezier curves.
	/// </summary>
	public sealed class PolyBezierSegment : PathSegment
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="PolyBezierSegment"/> class with an empty point collection.
		/// </summary>
		public PolyBezierSegment()
		{
			Points = new PointCollection();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="PolyBezierSegment"/> class with the specified points.
		/// </summary>
		/// <param name="points">The collection of control and end points for the Bezier curves.</param>
		public PolyBezierSegment(PointCollection points)
		{
			Points = points;
		}

		/// <summary>Bindable property for <see cref="Points"/>.</summary>
		public static readonly BindableProperty PointsProperty =
			BindableProperty.Create(nameof(Points), typeof(PointCollection), typeof(PolyBezierSegment), null);

		/// <summary>
		/// Gets or sets the collection of control and end points for the Bezier curves. This is a bindable property.
		/// </summary>
		public PointCollection Points
		{
			set { SetValue(PointsProperty, value); }
			get { return (PointCollection)GetValue(PointsProperty); }
		}
	}
}