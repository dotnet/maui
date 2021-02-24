using System;
using System.ComponentModel;
using CoreGraphics;
using FormsRectangle = Microsoft.Maui.Controls.Shapes.Rectangle;

#if __MOBILE__
namespace Microsoft.Maui.Controls.Compatibility.Platform.iOS
#else
namespace Microsoft.Maui.Controls.Compatibility.Platform.MacOS
#endif
{
    public class RectangleRenderer : ShapeRenderer<FormsRectangle, RectangleView>
    {
        // Each corner of the rounded rectangle is one-quarter of an ellipse with axes equal to the RadiusX and Radius parameters.
        const double MaximumRadius = 0.5d;

        [Microsoft.Maui.Controls.Internals.Preserve(Conditional = true)]
        public RectangleRenderer()
        {

        }

        protected override void OnElementChanged(ElementChangedEventArgs<FormsRectangle> args)
        {
            if (Control == null && args.NewElement != null)
            {
                SetNativeControl(new RectangleView());
            }

            base.OnElementChanged(args);
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            base.OnElementPropertyChanged(sender, args);

            if (args.PropertyName == VisualElement.HeightProperty.PropertyName || args.PropertyName == VisualElement.WidthProperty.PropertyName)
                UpdateRadius();
            if (args.PropertyName == FormsRectangle.RadiusXProperty.PropertyName)
                UpdateRadiusX();
            else if (args.PropertyName == FormsRectangle.RadiusYProperty.PropertyName)
                UpdateRadiusY();
        }

        void UpdateRadius()
        {
            UpdateRadiusX();
            UpdateRadiusY();
        }

        void UpdateRadiusX()
        {
            if (Element.Width > 0)
            {
                var radiusX = ValidateRadius(Element.RadiusX / Element.Width);
                Control.UpdateRadiusX(radiusX);
            }
        }

        void UpdateRadiusY()
        {
            if (Element.Height > 0)
            {
                var radiusY = ValidateRadius(Element.RadiusY / Element.Height);
                Control.UpdateRadiusY(radiusY);
            }
        }

        double ValidateRadius(double radius)
        {
            if (radius > MaximumRadius)
                radius = MaximumRadius;

            return radius;
        }
    }

    public class RectangleView : ShapeView
    {
        public RectangleView()
        {
            UpdateShape();
        }

        public nfloat RadiusX { set; get; }

        public nfloat RadiusY { set; get; }

        void UpdateShape()
        {
			var path = new CGPath();
            path.AddRoundedRect(new CGRect(0, 0, 1, 1), RadiusX, RadiusY);
            ShapeLayer.UpdateShape(path);
        }

        public void UpdateRadiusX(double radiusX)
        {
            if (double.IsInfinity(radiusX))
                return;

            RadiusX = new nfloat(radiusX);
            UpdateShape();
        }

        public void UpdateRadiusY(double radiusY)
        {
            if (double.IsInfinity(radiusY))
                return;

            RadiusY = new nfloat(radiusY);
            UpdateShape();
        }
    }
}