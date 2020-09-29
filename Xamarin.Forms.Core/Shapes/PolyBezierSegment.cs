namespace Xamarin.Forms.Shapes
{
	public sealed class PolyBezierSegment : PathSegment
	{
		public PolyBezierSegment()
		{
			Points = new PointCollection();
		}

		public PolyBezierSegment(PointCollection points)
		{
			Points = points;
		}

		public static readonly BindableProperty PointsProperty =
			BindableProperty.Create(nameof(Points), typeof(PointCollection), typeof(PolyBezierSegment), null);

		public PointCollection Points
		{
			set { SetValue(PointsProperty, value); }
			get { return (PointCollection)GetValue(PointsProperty); }
		}
	}
}