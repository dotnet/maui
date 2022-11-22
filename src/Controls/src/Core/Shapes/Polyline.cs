namespace Microsoft.Maui.Controls.Shapes
{
	/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/Polyline.xml" path="Type[@FullName='Microsoft.Maui.Controls.Shapes.Polyline']/Docs/*" />
	public sealed partial class Polyline : Shape
	{
		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/Polyline.xml" path="//Member[@MemberName='.ctor'][1]/Docs/*" />
		public Polyline() : base()
		{
		}

		public Polyline(PointCollection points) : this()
		{
			Points = points;
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/Polyline.xml" path="//Member[@MemberName='PointsProperty']/Docs/*" />
		public static readonly BindableProperty PointsProperty =
			BindableProperty.Create(nameof(Points), typeof(PointCollection), typeof(Polyline), null, defaultValueCreator: bindable => new PointCollection());

		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/Polyline.xml" path="//Member[@MemberName='FillRuleProperty']/Docs/*" />
		public static readonly BindableProperty FillRuleProperty =
			BindableProperty.Create(nameof(FillRule), typeof(FillRule), typeof(Polyline), FillRule.EvenOdd);

		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/Polyline.xml" path="//Member[@MemberName='Points']/Docs/*" />
		public PointCollection Points
		{
			set { SetValue(PointsProperty, value); }
			get { return (PointCollection)GetValue(PointsProperty); }
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/Polyline.xml" path="//Member[@MemberName='FillRule']/Docs/*" />
		public FillRule FillRule
		{
			set { SetValue(FillRuleProperty, value); }
			get { return (FillRule)GetValue(FillRuleProperty); }
		}
	}
}
