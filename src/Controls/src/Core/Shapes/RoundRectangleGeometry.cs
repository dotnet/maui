#nullable disable
using Microsoft.Maui.Devices;
using Microsoft.Maui.Graphics;
using Rect = Microsoft.Maui.Graphics.Rect;

namespace Microsoft.Maui.Controls.Shapes
{
	/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/RoundRectangleGeometry.xml" path="Type[@FullName='Microsoft.Maui.Controls.Shapes.RoundRectangleGeometry']/Docs/*" />
	public class RoundRectangleGeometry : GeometryGroup
	{
		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/RoundRectangleGeometry.xml" path="//Member[@MemberName='.ctor'][1]/Docs/*" />
		public RoundRectangleGeometry()
		{

		}

		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/RoundRectangleGeometry.xml" path="//Member[@MemberName='.ctor'][2]/Docs/*" />
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

		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/RoundRectangleGeometry.xml" path="//Member[@MemberName='Rect']/Docs/*" />
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

		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/RoundRectangleGeometry.xml" path="//Member[@MemberName='CornerRadius']/Docs/*" />
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