using System.ComponentModel;
using Android.Content;
using Android.Graphics;
using APath = Android.Graphics.Path;
using FormsRectangle = Xamarin.Forms.Shapes.Rectangle;

namespace Xamarin.Forms.Platform.Android
{
	public class RectangleRenderer : ShapeRenderer<FormsRectangle, RectView>
    {
        public RectangleRenderer(Context context) : base(context)
        {

        }

        protected override void OnElementChanged(ElementChangedEventArgs<FormsRectangle> args)
        {
            if (Control == null)
            {
                SetNativeControl(new RectView(Context));
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

            if (args.PropertyName == FormsRectangle.RadiusXProperty.PropertyName)
                UpdateRadiusX();
            else if (args.PropertyName == FormsRectangle.RadiusYProperty.PropertyName)
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
        public RectView(Context context) : base(context)
        {
            UpdateShape();
        }

        public float RadiusX { set; get; }

        public float RadiusY { set; get; }

        void UpdateShape()
        {
			var path = new APath();
            path.AddRoundRect(new RectF(0, 0, 1, 1), RadiusX, RadiusY, APath.Direction.Cw);
            UpdateShape(path);
        }

        public void UpdateRadiusX(double radiusX)
        {
            RadiusX = (float)radiusX;
            UpdateShape();
        }

        public void UpdateRadiusY(double radiusY)
        {
            RadiusY = (float)radiusY;
            UpdateShape();
        }
    }
}
