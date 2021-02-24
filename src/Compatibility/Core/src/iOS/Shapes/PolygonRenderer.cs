using System.Collections.Specialized;
using System.ComponentModel;
using CoreGraphics;
using Microsoft.Maui.Controls.Shapes;

#if __MOBILE__
namespace Microsoft.Maui.Controls.Compatibility.Platform.iOS
#else
namespace Microsoft.Maui.Controls.Compatibility.Platform.MacOS
#endif
{
    public class PolygonRenderer : ShapeRenderer<Polygon, PolygonView>
    {
        PointCollection _points;

        [Microsoft.Maui.Controls.Internals.Preserve(Conditional = true)]
        public PolygonRenderer()
        {

        }

        protected override void OnElementChanged(ElementChangedEventArgs<Polygon> args)
        {
            if (Control == null && args.NewElement != null)
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
                if (_points != null)
                {
                    _points.CollectionChanged -= OnCollectionChanged;
                    _points = null;
                }
            }
        }

        void UpdatePoints()
        {
            if (_points != null)
                _points.CollectionChanged -= OnCollectionChanged;

            _points = Element.Points;

            _points.CollectionChanged += OnCollectionChanged;

            Control.UpdatePoints(_points.ToCGPoints());
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