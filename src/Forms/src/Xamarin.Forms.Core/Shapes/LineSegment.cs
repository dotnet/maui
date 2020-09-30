namespace Xamarin.Forms.Shapes
{
	public class LineSegment : PathSegment
	{
		public LineSegment()
		{

		}

		public LineSegment(Point point)
		{
			Point = point;
		}

		public static readonly BindableProperty PointProperty =
			BindableProperty.Create(nameof(Point), typeof(Point), typeof(LineSegment), new Point(0, 0));

		public Point Point
		{
			set { SetValue(PointProperty, value); }
			get { return (Point)GetValue(PointProperty); }
		}
	}
}