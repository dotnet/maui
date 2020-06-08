using System;
using System.ComponentModel;
using CoreGraphics;
using Rect = Xamarin.Forms.Shapes.Rectangle;

#if __MOBILE__
namespace Xamarin.Forms.Platform.iOS
#else
namespace Xamarin.Forms.Platform.MacOS
#endif
{
    public class RectangleRenderer : ShapeRenderer<Rect, RectView>
    {
        protected override void OnElementChanged(ElementChangedEventArgs<Rect> args)
        {
            if (Control == null)
            {
                SetNativeControl(new RectView());
            }

            base.OnElementChanged(args);

            if (args.NewElement != null)
            {
                UpdateRadiusX();
                UpdateRadiusY();
            }
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            base.OnElementPropertyChanged(sender, args);

            if (args.PropertyName == Rect.RadiusXProperty.PropertyName)
                UpdateRadiusX();
            else if (args.PropertyName == Rect.RadiusYProperty.PropertyName)
                UpdateRadiusY();
        }

        void UpdateRadiusX()
        {
            Control.UpdateRadiusX(Element.RadiusX / Element.WidthRequest);
        }

        void UpdateRadiusY()
        {
            Control.UpdateRadiusY(Element.RadiusY / Element.HeightRequest);
        }
    }

    public class RectView : ShapeView
    {
        public RectView()
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