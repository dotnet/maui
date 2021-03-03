namespace Microsoft.Maui.Controls.Shapes
{
	public class RoundRectangleGeometry : GeometryGroup
	{
		public RoundRectangleGeometry()
		{

		}

		public RoundRectangleGeometry(CornerRadius cornerRadius, Rect rect)
		{
			CornerRadius = cornerRadius;
			Rect = rect;
		}

		public static readonly BindableProperty RectProperty =
		   BindableProperty.Create(nameof(Rect), typeof(Rect), typeof(RoundRectangleGeometry), new Rect(),
			   propertyChanged: OnRectChanged);

		static void OnRectChanged(BindableObject bindable, object oldValue, object newValue)
		{
			(bindable as RoundRectangleGeometry)?.UpdateGeometry();
		}

		public Rect Rect
		{
			set { SetValue(RectProperty, value); }
			get { return (Rect)GetValue(RectProperty); }
		}

		public static readonly BindableProperty CornerRadiusProperty =
			BindableProperty.Create(nameof(CornerRadius), typeof(CornerRadius), typeof(RoundRectangleGeometry), new CornerRadius(),
				propertyChanged: OnCornerRadiusChanged);

		static void OnCornerRadiusChanged(BindableObject bindable, object oldValue, object newValue)
		{
			(bindable as RoundRectangleGeometry)?.UpdateGeometry();
		}

		public CornerRadius CornerRadius
		{
			set { SetValue(CornerRadiusProperty, value); }
			get { return (CornerRadius)GetValue(CornerRadiusProperty); }
		}

		void UpdateGeometry()
		{
			FillRule = FillRule.Nonzero;

			Children.Clear();

			Children.Add(GetRoundRectangleGeometry());
		}

		Geometry GetRoundRectangleGeometry()
		{
			GeometryGroup roundedRectGeometry = new GeometryGroup
			{
				FillRule = FillRule.Nonzero
			};

			if (CornerRadius.TopLeft > 0)
				roundedRectGeometry.Children.Add(
					new EllipseGeometry(new Point(Rect.Location.X + CornerRadius.TopLeft, Rect.Location.Y + CornerRadius.TopLeft), CornerRadius.TopLeft, CornerRadius.TopLeft));

			if (CornerRadius.TopRight > 0)
				roundedRectGeometry.Children.Add(
					new EllipseGeometry(new Point(Rect.Location.X + Rect.Width - CornerRadius.TopRight, Rect.Location.Y + CornerRadius.TopRight), CornerRadius.TopRight, CornerRadius.TopRight));

			if (CornerRadius.BottomRight > 0)
				roundedRectGeometry.Children.Add(
					new EllipseGeometry(new Point(Rect.Location.X + Rect.Width - CornerRadius.BottomRight, Rect.Location.Y + Rect.Height - CornerRadius.BottomRight), CornerRadius.BottomRight, CornerRadius.BottomRight));

			if (CornerRadius.BottomLeft > 0)
				roundedRectGeometry.Children.Add(
					new EllipseGeometry(new Point(Rect.Location.X + CornerRadius.BottomLeft, Rect.Location.Y + Rect.Height - CornerRadius.BottomLeft), CornerRadius.BottomLeft, CornerRadius.BottomLeft));

			PathFigure pathFigure = new PathFigure
			{
				IsClosed = true,
				StartPoint = new Point(Rect.Location.X + CornerRadius.TopLeft, Rect.Location.Y),
				Segments = new PathSegmentCollection
				{
					new LineSegment { Point = new Point(Rect.Location.X + Rect.Width - CornerRadius.TopRight, Rect.Location.Y) },
					new LineSegment { Point = new Point(Rect.Location.X + Rect.Width, Rect.Location.Y + CornerRadius.TopRight) },
					new LineSegment { Point = new Point(Rect.Location.X + Rect.Width, Rect.Location.Y + Rect.Height - CornerRadius.BottomRight) },
					new LineSegment { Point = new Point(Rect.Location.X + Rect.Width - CornerRadius.BottomRight, Rect.Location.Y + Rect.Height) },
					new LineSegment { Point = new Point(Rect.Location.X + CornerRadius.BottomLeft, Rect.Location.Y + Rect.Height) },
					new LineSegment { Point = new Point(Rect.Location.X, Rect.Location.Y + Rect.Height - CornerRadius.BottomLeft) },
					new LineSegment { Point = new Point(Rect.Location.X, Rect.Location.Y + CornerRadius.TopLeft) }
				}
			};

			PathFigureCollection pathFigureCollection = new PathFigureCollection
			{
				pathFigure
			};

			roundedRectGeometry.Children.Add(new PathGeometry(pathFigureCollection, FillRule.Nonzero));

			return roundedRectGeometry;
		}
	}
}