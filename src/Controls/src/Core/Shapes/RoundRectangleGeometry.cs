#nullable disable
using Microsoft.Maui.Devices;
using Microsoft.Maui.Graphics;
using Rect = Microsoft.Maui.Graphics.Rect;

namespace Microsoft.Maui.Controls.Shapes
{
	/// <summary>
	/// Represents a geometry that describes a rounded rectangle.
	/// </summary>
	public class RoundRectangleGeometry : GeometryGroup
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="RoundRectangleGeometry"/> class.
		/// </summary>
		public RoundRectangleGeometry()
		{

		}

		/// <summary>
		/// Initializes a new instance of the <see cref="RoundRectangleGeometry"/> class with the specified corner radius and rectangle.
		/// </summary>
		/// <param name="cornerRadius">The corner radius for the rounded rectangle.</param>
		/// <param name="rect">The rectangle that defines the bounds of the geometry.</param>
		public RoundRectangleGeometry(CornerRadius cornerRadius, Rect rect)
		{
			CornerRadius = cornerRadius;
			Rect = rect;
		}

		/// <summary>Bindable property for <see cref="Rect"/>.</summary>
		public static readonly BindableProperty RectProperty =
		   BindableProperty.Create(nameof(Rect), typeof(Rect), typeof(RoundRectangleGeometry), new Rect(),
			   propertyChanged: OnRectChanged);

		static void OnRectChanged(BindableObject bindable, object oldValue, object newValue)
		{
			(bindable as RoundRectangleGeometry)?.UpdateGeometry();
		}

		/// <summary>
		/// Gets or sets the rectangle that defines the bounds of the geometry.
		/// </summary>
		/// <value>A <see cref="Rect"/> that defines the bounds of the geometry.</value>
		public Rect Rect
		{
			set { SetValue(RectProperty, value); }
			get { return (Rect)GetValue(RectProperty); }
		}

		/// <summary>Bindable property for <see cref="CornerRadius"/>.</summary>
		public static readonly BindableProperty CornerRadiusProperty =
			BindableProperty.Create(nameof(CornerRadius), typeof(CornerRadius), typeof(RoundRectangleGeometry), new CornerRadius(),
				propertyChanged: OnCornerRadiusChanged);

		static void OnCornerRadiusChanged(BindableObject bindable, object oldValue, object newValue)
		{
			(bindable as RoundRectangleGeometry)?.UpdateGeometry();
		}

		/// <summary>
		/// Gets or sets the corner radius for the round rectangle geometry.
		/// </summary>
		/// <value>A <see cref="CornerRadius"/> value that specifies the radius for each corner of the round rectangle geometry.</value>
		/// <remarks>When specifying corner radii, the order of values is top left, top right, bottom left, and bottom right.</remarks>
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

		public override void AppendPath(Graphics.PathF path)
		{
			float x = (float)Rect.X;
			float y = (float)Rect.Y;
			float w = (float)Rect.Width;
			float h = (float)Rect.Height;

			float tl = (float)CornerRadius.TopLeft;
			float tr = (float)CornerRadius.TopRight;
			float bl = (float)CornerRadius.BottomLeft;
			float br = (float)CornerRadius.BottomRight;

			path.AppendRoundedRectangle(x, y, w, h, tl, tr, bl, br);
		}
	}
}