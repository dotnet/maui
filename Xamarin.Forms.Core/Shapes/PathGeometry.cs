namespace Xamarin.Forms.Shapes
{
    [ContentProperty("Figures")]
    public sealed class PathGeometry : Geometry
    {
        public PathGeometry()
        {
            Figures = new PathFigureCollection();
        }

        public PathGeometry(PathFigureCollection figures)
        {
            Figures = figures;
        }

        public PathGeometry(PathFigureCollection figures, FillRule fillRule)
        {
            Figures = figures;
            FillRule = fillRule;
        }

        public static readonly BindableProperty FiguresProperty =
            BindableProperty.Create(nameof(Figures), typeof(PathFigureCollection), typeof(PathGeometry), null);

        public static readonly BindableProperty FillRuleProperty =
            BindableProperty.Create(nameof(FillRule), typeof(FillRule), typeof(PathGeometry), FillRule.EvenOdd);

        [TypeConverter(typeof(PathFigureCollectionConverter))]
        public PathFigureCollection Figures
        {
            set { SetValue(FiguresProperty, value); }
            get { return (PathFigureCollection)GetValue(FiguresProperty); }
        }

        public FillRule FillRule
        {
            set { SetValue(FillRuleProperty, value); }
            get { return (FillRule)GetValue(FillRuleProperty); }
        }
    }
}