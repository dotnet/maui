namespace Xamarin.Forms.Shapes
{
    public class PolyQuadraticBezierSegment : PathSegment
    {
        public PolyQuadraticBezierSegment()
        {
            Points = new PointCollection();
        }

        public static readonly BindableProperty PointsProperty =
            BindableProperty.Create(nameof(Points), typeof(PointCollection), typeof(PolyQuadraticBezierSegment), null);

        public PointCollection Points
        {
            set { SetValue(PointsProperty, value); }
            get { return (PointCollection)GetValue(PointsProperty); }
        }
    }
}