namespace Xamarin.Forms.Shapes
{
	public class ArcSegment : PathSegment
	{
		public ArcSegment()
		{

		}

		public ArcSegment(Point point, Size size, double rotationAngle, SweepDirection sweepDirection, bool isLargeArc)
		{
			Point = point;
			Size = size;
			RotationAngle = rotationAngle;
			SweepDirection = sweepDirection;
			IsLargeArc = isLargeArc;
		}

		public static readonly BindableProperty PointProperty =
			BindableProperty.Create(nameof(Point), typeof(Point), typeof(ArcSegment), new Point(0, 0));

		public static readonly BindableProperty SizeProperty =
			BindableProperty.Create(nameof(Size), typeof(Size), typeof(ArcSegment), new Size(0, 0));

		public static readonly BindableProperty RotationAngleProperty =
			BindableProperty.Create(nameof(RotationAngle), typeof(double), typeof(ArcSegment), 0.0);

		public static readonly BindableProperty SweepDirectionProperty =
			BindableProperty.Create(nameof(SweepDirection), typeof(SweepDirection), typeof(ArcSegment), SweepDirection.CounterClockwise);

		public static readonly BindableProperty IsLargeArcProperty =
			BindableProperty.Create(nameof(IsLargeArc), typeof(bool), typeof(ArcSegment), false);

		public Point Point
		{
			set { SetValue(PointProperty, value); }
			get { return (Point)GetValue(PointProperty); }
		}

		[TypeConverter(typeof(SizeTypeConverter))]
		public Size Size
		{
			set { SetValue(SizeProperty, value); }
			get { return (Size)GetValue(SizeProperty); }
		}

		public double RotationAngle
		{
			set { SetValue(RotationAngleProperty, value); }
			get { return (double)GetValue(RotationAngleProperty); }
		}

		public SweepDirection SweepDirection
		{
			set { SetValue(SweepDirectionProperty, value); }
			get { return (SweepDirection)GetValue(SweepDirectionProperty); }
		}

		public bool IsLargeArc
		{
			set { SetValue(IsLargeArcProperty, value); }
			get { return (bool)GetValue(IsLargeArcProperty); }
		}
	}
}