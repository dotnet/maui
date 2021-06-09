namespace Microsoft.Maui.Controls.Shapes
{
	public sealed partial class Polygon : Shape
	{
		public Polygon() : base()
		{
		}

		public Polygon(PointCollection points) : this()
		{
			Points = points;
		}

		public static readonly BindableProperty PointsProperty =
			BindableProperty.Create(nameof(Points), typeof(PointCollection), typeof(Polygon), null, defaultValueCreator: bindable => new PointCollection());

		public static readonly BindableProperty FillRuleProperty =
			BindableProperty.Create(nameof(FillRule), typeof(FillRule), typeof(Polygon), FillRule.EvenOdd);

		public PointCollection Points
		{
			set { SetValue(PointsProperty, value); }
			get { return (PointCollection)GetValue(PointsProperty); }
		}

		public FillRule FillRule
		{
			set { SetValue(FillRuleProperty, value); }
			get { return (FillRule)GetValue(FillRuleProperty); }

		}
	}
}