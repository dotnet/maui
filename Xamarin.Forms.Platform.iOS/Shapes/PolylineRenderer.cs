using System.ComponentModel;
using CoreGraphics;
using Xamarin.Forms.Shapes;

#if __MOBILE__
namespace Xamarin.Forms.Platform.iOS
#else
namespace Xamarin.Forms.Platform.MacOS
#endif
{
    public class PolylineRenderer : ShapeRenderer<Polyline, PolylineView>
    {
        [Internals.Preserve(Conditional = true)]
        public PolylineRenderer()
        {

        }

        protected override void OnElementChanged(ElementChangedEventArgs<Polyline> args)
        {
            if (Control == null)
            {
                SetNativeControl(new PolylineView());
            }

            base.OnElementChanged(args);

            if (args.NewElement != null)
            {
                UpdatePoints();
                UpdateFillRule();
            }
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            base.OnElementPropertyChanged(sender, args);

            if (args.PropertyName == Polyline.PointsProperty.PropertyName)
                UpdatePoints();
            else if (args.PropertyName == Polyline.FillRuleProperty.PropertyName)
                UpdateFillRule();
        }

        void UpdatePoints()
        {
            Control.UpdatePoints(Element.Points.ToCGPoints());
        }

        public void UpdateFillRule()
        {
            Control.UpdateFillMode(Element.FillRule == FillRule.Nonzero);
        }
    }

    public class PolylineView : ShapeView
    {
        public void UpdatePoints(CGPoint[] points)
        {
			var path = new CGPath();
            path.AddLines(points);
            ShapeLayer.UpdateShape(path);
        }

        public void UpdateFillMode(bool fillMode)
        {
            ShapeLayer.UpdateFillMode(fillMode);
        }
    }
}
