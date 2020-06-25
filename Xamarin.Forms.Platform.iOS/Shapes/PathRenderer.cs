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
            if (Control == null)
            {
                SetNativeControl(new PathView());
            }

            base.OnElementChanged(args);

            if (args.NewElement != null)
            {
                UpdateData();
                UpdateRenderTransform();
            }
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            base.OnElementPropertyChanged(sender, args);

            if (args.PropertyName == Path.DataProperty.PropertyName)
                UpdateData();
            else if (args.PropertyName == Path.RenderTransformProperty.PropertyName)
                UpdateRenderTransform();
        }

        void UpdateData()
        {
            Control.UpdateData(Element.Data.ToCGPath());
        }

        void UpdateRenderTransform()
        {
            if (Element.RenderTransform != null)
                Control.UpdateTransform(Element.RenderTransform.ToCGAffineTransform());
        }
    }

	public class PathData
    {
        public CGPath Data { get; set; }
        public bool IsNonzeroFillRule { get; set; }
    }

    public class PathView : ShapeView
    {
        public void UpdateData(PathData path)
        {
            ShapeLayer.UpdateShape(path.Data);
            ShapeLayer.UpdateFillMode(path == null ? false : path.IsNonzeroFillRule);
        }

		public void UpdateTransform(CGAffineTransform transform)
        {
#if __MOBILE__
            Transform = transform;
#else
            WantsLayer = true;
            Layer.AffineTransform = transform;
#endif
        }
    }
}