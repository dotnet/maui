namespace Xamarin.Forms.Shapes
{
	public class PolyLineSegment : PathSegment
	{
		public PolyLineSegment()
		{
			Points = new PointCollection();
		}

		public PolyLineSegment(PointCollection points)
		{
			Points = points;
		}

		public static readonly BindableProperty PointsProperty =
			BindableProperty.Create(nameof(Points), typeof(PointCollection), typeof(PolyLineSegment), null);

		public PointCollection Points
		{
			set { SetValue(PointsProperty, value); }
			get { return (PointCollection)GetValue(PointsProperty); }
		}
	}
}