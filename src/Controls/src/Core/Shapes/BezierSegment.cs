#nullable disable
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Shapes
{
	/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/BezierSegment.xml" path="Type[@FullName='Microsoft.Maui.Controls.Shapes.BezierSegment']/Docs/*" />
	public class BezierSegment : PathSegment
	{
		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/BezierSegment.xml" path="//Member[@MemberName='.ctor'][1]/Docs/*" />
		public BezierSegment()
		{

		}

		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/BezierSegment.xml" path="//Member[@MemberName='.ctor'][2]/Docs/*" />
		public BezierSegment(Point point1, Point point2, Point point3)
		{
			Point1 = point1;
			Point2 = point2;
			Point3 = point3;
		}

		/// <summary>Bindable property for <see cref="Point1"/>.</summary>
		public static readonly BindableProperty Point1Property =
			BindableProperty.Create(nameof(Point1), typeof(Point), typeof(BezierSegment), new Point(0, 0));

		/// <summary>Bindable property for <see cref="Point2"/>.</summary>
		public static readonly BindableProperty Point2Property =
			BindableProperty.Create(nameof(Point2), typeof(Point), typeof(BezierSegment), new Point(0, 0));

		/// <summary>Bindable property for <see cref="Point3"/>.</summary>
		public static readonly BindableProperty Point3Property =
			BindableProperty.Create(nameof(Point3), typeof(Point), typeof(BezierSegment), new Point(0, 0));

		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/BezierSegment.xml" path="//Member[@MemberName='Point1']/Docs/*" />
		public Point Point1
		{
			set { SetValue(Point1Property, value); }
			get { return (Point)GetValue(Point1Property); }
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/BezierSegment.xml" path="//Member[@MemberName='Point2']/Docs/*" />
		public Point Point2
		{
			set { SetValue(Point2Property, value); }
			get { return (Point)GetValue(Point2Property); }
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/BezierSegment.xml" path="//Member[@MemberName='Point3']/Docs/*" />
		public Point Point3
		{
			set { SetValue(Point3Property, value); }
			get { return (Point)GetValue(Point3Property); }
		}
	}
}