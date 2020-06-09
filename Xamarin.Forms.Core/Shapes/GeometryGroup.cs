namespace Xamarin.Forms.Shapes
{
    [ContentProperty("Children")]
    public sealed class GeometryGroup : Geometry
    {
        public static readonly BindableProperty ChildrenProperty =
            BindableProperty.Create(nameof(Children), typeof(GeometryCollection), typeof(GeometryGroup), null);

        public static readonly BindableProperty FillRuleProperty =
            BindableProperty.Create(nameof(FillRule), typeof(FillRule), typeof(GeometryGroup), FillRule.EvenOdd);

        public GeometryGroup()
        {
            Children = new GeometryCollection();
        }

        public GeometryCollection Children
        {
            set { SetValue(ChildrenProperty, value); }
            get { return (GeometryCollection)GetValue(ChildrenProperty); }
        }

        public FillRule FillRule
        {
            set { SetValue(FillRuleProperty, value); }
            get { return (FillRule)GetValue(FillRuleProperty); }
        }
    }
}