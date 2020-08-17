using System;
using System.ComponentModel;
using CoreGraphics;
using FormsRectangle = Xamarin.Forms.Shapes.Rectangle;

#if __MOBILE__
namespace Xamarin.Forms.Platform.iOS
#else
namespace Xamarin.Forms.Platform.MacOS
#endif
{
    public class RectangleRenderer : ShapeRenderer<FormsRectangle, RectangleView>
    {
        [Internals.Preserve(Conditional = true)]
        public RectangleRenderer()
        {

        }

        protected override void OnElementChanged(ElementChangedEventArgs<FormsRectangle> args)
        {
            if (Control == null)
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
                Control.UpdateRadiusX(Element.RadiusX / Element.Width);
        }

        void UpdateRadiusY()
        {
            if (Element.Height > 0)
                Control.UpdateRadiusY(Element.RadiusY / Element.Height);
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