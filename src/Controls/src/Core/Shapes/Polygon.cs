namespace Microsoft.Maui.Controls.Shapes
{
	/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/Polygon.xml" path="Type[@FullName='Microsoft.Maui.Controls.Shapes.Polygon']/Docs/*" />
	public sealed partial class Polygon : Shape
	{
		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/Polygon.xml" path="//Member[@MemberName='.ctor'][1]/Docs/*" />
		public Polygon() : base()
		{
		}

		public Polygon(PointCollection points) : this()
		{
			Points = points;
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/Polygon.xml" path="//Member[@MemberName='PointsProperty']/Docs/*" />
		public static readonly BindableProperty PointsProperty =
			BindableProperty.Create(nameof(Points), typeof(PointCollection), typeof(Polygon), null, defaultValueCreator: bindable => new PointCollection());

		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/Polygon.xml" path="//Member[@MemberName='FillRuleProperty']/Docs/*" />
		public static readonly BindableProperty FillRuleProperty =
			BindableProperty.Create(nameof(FillRule), typeof(FillRule), typeof(Polygon), FillRule.EvenOdd);

		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/Polygon.xml" path="//Member[@MemberName='Points']/Docs/*" />
		public PointCollection Points
		{
			set { SetValue(PointsProperty, value); }
			get { return (PointCollection)GetValue(PointsProperty); }
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/Polygon.xml" path="//Member[@MemberName='FillRule']/Docs/*" />
		public FillRule FillRule
		{
			set { SetValue(FillRuleProperty, value); }
			get { return (FillRule)GetValue(FillRuleProperty); }

		}
	}
}
