using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Shapes
{
	/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/QuadraticBezierSegment.xml" path="Type[@FullName='Microsoft.Maui.Controls.Shapes.QuadraticBezierSegment']/Docs" />
	public class QuadraticBezierSegment : PathSegment
	{
		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/QuadraticBezierSegment.xml" path="//Member[@MemberName='.ctor'][1]/Docs" />
		public QuadraticBezierSegment()
		{

		}

		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/QuadraticBezierSegment.xml" path="//Member[@MemberName='.ctor'][2]/Docs" />
		public QuadraticBezierSegment(Point point1, Point point2)
		{
			Point1 = point1;
			Point2 = point2;
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/QuadraticBezierSegment.xml" path="//Member[@MemberName='Point1Property']/Docs" />
		public static readonly BindableProperty Point1Property =
			BindableProperty.Create(nameof(Point1), typeof(Point), typeof(QuadraticBezierSegment), new Point(0, 0));

		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/QuadraticBezierSegment.xml" path="//Member[@MemberName='Point2Property']/Docs" />
		public static readonly BindableProperty Point2Property =
			BindableProperty.Create(nameof(Point2), typeof(Point), typeof(QuadraticBezierSegment), new Point(0, 0));

		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/QuadraticBezierSegment.xml" path="//Member[@MemberName='Point1']/Docs" />
		public Point Point1
		{
			set { SetValue(Point1Property, value); }
			get { return (Point)GetValue(Point1Property); }
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/QuadraticBezierSegment.xml" path="//Member[@MemberName='Point2']/Docs" />
		public Point Point2
		{
			set { SetValue(Point2Property, value); }
			get { return (Point)GetValue(Point2Property); }
		}
	}
}