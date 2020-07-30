using System.Collections.Specialized;
using System.ComponentModel;
using CoreGraphics;
using Xamarin.Forms.Shapes;

#if __MOBILE__
namespace Xamarin.Forms.Platform.iOS
#else
namespace Xamarin.Forms.Platform.MacOS
#endif
{
    public class PolygonRenderer : ShapeRenderer<Polygon, PolygonView>
    {
        [Internals.Preserve(Conditional = true)]
        public PolygonRenderer()
        {

        }

        protected override void OnElementChanged(ElementChangedEventArgs<Polygon> args)
        {
            if (Control == null)
            {
                SetNativeControl(new PolygonView());
            }

            base.OnElementChanged(args);

            if (args.NewElement != null)
            {
                var points = args.NewElement.Points;
                points.CollectionChanged += OnCollectionChanged;

                UpdatePoints();
                UpdateFillRule();
            }
        }

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            base.OnElementPropertyChanged(sender, args);

            if (args.PropertyName == Polygon.PointsProperty.PropertyName)
                UpdatePoints();
            else if (args.PropertyName == Polygon.FillRuleProperty.PropertyName)
                UpdateFillRule();
        }

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

            if (disposing)
            {
                if (Element != null)
                {
                    var points = Element.Points;
                    points.CollectionChanged -= OnCollectionChanged;
                }
            }
		}

		void UpdatePoints()
        {
            Control.UpdatePoints(Element.Points.ToCGPoints());
        }

        public void UpdateFillRule()
        {
            Control.UpdateFillMode(Element.FillRule == FillRule.Nonzero);
        }

        void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            UpdatePoints();
        }
    }

    public class PolygonView : ShapeView
    {
        public void UpdatePoints(CGPoint[] points)
        {
			var path = new CGPath();
            path.AddLines(points);
            path.CloseSubpath();

            ShapeLayer.UpdateShape(path);
        }

        public void UpdateFillMode(bool fillMode)
        {
            ShapeLayer.UpdateFillMode(fillMode);
        }
    }
}