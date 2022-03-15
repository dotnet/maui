namespace Microsoft.Maui.Controls.Shapes
{
	/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/PolyBezierSegment.xml" path="Type[@FullName='Microsoft.Maui.Controls.Shapes.PolyBezierSegment']/Docs" />
	public sealed class PolyBezierSegment : PathSegment
	{
		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/PolyBezierSegment.xml" path="//Member[@MemberName='.ctor'][1]/Docs" />
		public PolyBezierSegment()
		{
			Points = new PointCollection();
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/PolyBezierSegment.xml" path="//Member[@MemberName='.ctor'][2]/Docs" />
		public PolyBezierSegment(PointCollection points)
		{
			Points = points;
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/PolyBezierSegment.xml" path="//Member[@MemberName='PointsProperty']/Docs" />
		public static readonly BindableProperty PointsProperty =
			BindableProperty.Create(nameof(Points), typeof(PointCollection), typeof(PolyBezierSegment), null);

		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/PolyBezierSegment.xml" path="//Member[@MemberName='Points']/Docs" />
		public PointCollection Points
		{
			set { SetValue(PointsProperty, value); }
			get { return (PointCollection)GetValue(PointsProperty); }
		}
	}
}