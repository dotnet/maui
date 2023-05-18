#nullable disable
namespace Microsoft.Maui.Controls.Shapes
{
	/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/PolyQuadraticBezierSegment.xml" path="Type[@FullName='Microsoft.Maui.Controls.Shapes.PolyQuadraticBezierSegment']/Docs/*" />
	public class PolyQuadraticBezierSegment : PathSegment
	{
		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/PolyQuadraticBezierSegment.xml" path="//Member[@MemberName='.ctor'][1]/Docs/*" />
		public PolyQuadraticBezierSegment()
		{
			Points = new PointCollection();
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/PolyQuadraticBezierSegment.xml" path="//Member[@MemberName='.ctor'][2]/Docs/*" />
		public PolyQuadraticBezierSegment(PointCollection points)
		{
			Points = points;
		}

		/// <summary>Bindable property for <see cref="Points"/>.</summary>
		public static readonly BindableProperty PointsProperty =
			BindableProperty.Create(nameof(Points), typeof(PointCollection), typeof(PolyQuadraticBezierSegment), null);

		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/PolyQuadraticBezierSegment.xml" path="//Member[@MemberName='Points']/Docs/*" />
		public PointCollection Points
		{
			set { SetValue(PointsProperty, value); }
			get { return (PointCollection)GetValue(PointsProperty); }
		}
	}
}