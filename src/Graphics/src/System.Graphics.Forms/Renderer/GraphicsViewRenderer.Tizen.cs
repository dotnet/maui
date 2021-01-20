using System.ComponentModel;
using System.Graphics.Skia.Views;
using System.Graphics.Forms;
using System.Graphics.Forms.Tizen;
using Xamarin.Forms;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Platform.Tizen;
using XForms = Xamarin.Forms.Forms;

[assembly: ExportRenderer(typeof(GraphicsView), typeof(GraphicsViewRenderer))]
namespace System.Graphics.Forms.Tizen
{
    [Preserve]
    public class GraphicsViewRenderer : ViewRenderer<GraphicsView, SkiaGraphicsView>
    {
        public GraphicsViewRenderer()
        {
            RegisterPropertyHandler(GraphicsView.DrawableProperty, UpdateDrawable);
        }

        protected override void OnElementChanged(ElementChangedEventArgs<GraphicsView> e)
        {
            if (e.OldElement != null)
            {
                // Unsubscribe from event handlers and cleanup any resources
                SetNativeControl(null);
            }

            if (e.NewElement != null)
            {
                SetNativeControl(new SkiaGraphicsView(XForms.NativeParent));
            }
            base.OnElementChanged(e);
        }

        private void UpdateDrawable()
        {
            Control.Drawable = Element.Drawable;
        }
    }
}