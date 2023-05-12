#nullable disable
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Shapes
{
	/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/LineSegment.xml" path="Type[@FullName='Microsoft.Maui.Controls.Shapes.LineSegment']/Docs/*" />
	public class LineSegment : PathSegment
	{
		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/LineSegment.xml" path="//Member[@MemberName='.ctor'][1]/Docs/*" />
		public LineSegment()
		{

		}

		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/LineSegment.xml" path="//Member[@MemberName='.ctor'][2]/Docs/*" />
		public LineSegment(Point point)
		{
			Point = point;
		}

		/// <summary>Bindable property for <see cref="Point"/>.</summary>
		public static readonly BindableProperty PointProperty =
			BindableProperty.Create(nameof(Point), typeof(Point), typeof(LineSegment), new Point(0, 0));

		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/LineSegment.xml" path="//Member[@MemberName='Point']/Docs/*" />
		public Point Point
		{
			set { SetValue(PointProperty, value); }
			get { return (Point)GetValue(PointProperty); }
		}
	}
}