#nullable disable
namespace Microsoft.Maui.Controls.Shapes
{
	/// <summary>
	/// A path segment that draws a series of connected straight lines.
	/// </summary>
	public class PolyLineSegment : PathSegment
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="PolyLineSegment"/> class with an empty point collection.
		/// </summary>
		public PolyLineSegment()
		{
			Points = new PointCollection();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="PolyLineSegment"/> class with the specified points.
		/// </summary>
		/// <param name="points">The endpoints of the line segments.</param>
		public PolyLineSegment(PointCollection points)
		{
			Points = points;
		}

		/// <summary>Bindable property for <see cref="Points"/>.</summary>
		public static readonly BindableProperty PointsProperty =
			BindableProperty.Create(nameof(Points), typeof(PointCollection), typeof(PolyLineSegment), null);

		/// <summary>
		/// Gets or sets the endpoints of the line segments. This is a bindable property.
		/// </summary>
		public PointCollection Points
		{
			set { SetValue(PointsProperty, value); }
			get { return (PointCollection)GetValue(PointsProperty); }
		}
	}
}