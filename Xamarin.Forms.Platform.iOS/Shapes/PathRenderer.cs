using System.ComponentModel;
using CoreGraphics;
using Path = Xamarin.Forms.Shapes.Path;

#if __MOBILE__
namespace Xamarin.Forms.Platform.iOS
#else
namespace Xamarin.Forms.Platform.MacOS
#endif
{
    public class PathRenderer : ShapeRenderer<Path, PathView>
    {
        [Internals.Preserve(Conditional = true)]
        public PathRenderer()
        {

        }

        protected override void OnElementChanged(ElementChangedEventArgs<Path> args)
        {
            if (Control == null && args.NewElement != null)
            {
                SetNativeControl(new PathView());
            }

            base.OnElementChanged(args);

            if (args.NewElement != null)
            {
                UpdatePath();
            }
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            base.OnElementPropertyChanged(sender, args);

            if (args.PropertyName == Path.DataProperty.PropertyName || args.PropertyName == Path.RenderTransformProperty.PropertyName)
                UpdatePath();
        }

        void UpdatePath()
        {
            Control.UpdatePath(Element.Data.ToCGPath(Element.RenderTransform));
        }
    }

	public class PathData
    {
        public CGPath Data { get; set; }
        public bool IsNonzeroFillRule { get; set; }
    }

    public class PathView : ShapeView
    {
        public void UpdatePath(PathData path)
        {
            ShapeLayer.UpdateShape(path.Data);
            ShapeLayer.UpdateFillMode(path != null && path.IsNonzeroFillRule);
        }
    }
}