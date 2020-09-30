using CoreGraphics;
using Xamarin.Forms.Shapes;

#if __MOBILE__
namespace Xamarin.Forms.Platform.iOS
#else
namespace Xamarin.Forms.Platform.MacOS
#endif
{
    public class EllipseRenderer : ShapeRenderer<Ellipse, EllipseView>
    {
        [Internals.Preserve(Conditional = true)]
        public EllipseRenderer()
        {

        }

        protected override void OnElementChanged(ElementChangedEventArgs<Ellipse> args)
        {
            if (Control == null)
            {
                SetNativeControl(new EllipseView());
            }

            base.OnElementChanged(args);
        }
    }

    public class EllipseView : ShapeView
    {
        public EllipseView()
        {
            UpdateShape();
        }

        void UpdateShape()
        {
			var path = new CGPath();
            path.AddEllipseInRect(new CGRect(0, 0, 1, 1));
            ShapeLayer.UpdateShape(path);
        }
    }
}