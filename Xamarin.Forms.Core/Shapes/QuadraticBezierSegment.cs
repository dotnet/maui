namespace Xamarin.Forms.Shapes
{
	public class QuadraticBezierSegment : PathSegment
	{
		public QuadraticBezierSegment()
		{

		}

		public QuadraticBezierSegment(Point point1, Point point2)
		{
			Point1 = point1;
			Point2 = point2;
		}

		public static readonly BindableProperty Point1Property =
			BindableProperty.Create(nameof(Point1), typeof(Point), typeof(QuadraticBezierSegment), new Point(0, 0));

		public static readonly BindableProperty Point2Property =
			BindableProperty.Create(nameof(Point2), typeof(Point), typeof(QuadraticBezierSegment), new Point(0, 0));

		public Point Point1
		{
			set { SetValue(Point1Property, value); }
			get { return (Point)GetValue(Point1Property); }
		}

		public Point Point2
		{
			set { SetValue(Point2Property, value); }
			get { return (Point)GetValue(Point2Property); }
		}
	}
}