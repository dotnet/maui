using System.ComponentModel;
using System.Graphics.CoreGraphics;
using System.Graphics.Forms;
using System.Graphics.Forms.iOS;
using Foundation;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(GraphicsView), typeof(GraphicsViewRenderer))]

namespace System.Graphics.Forms.iOS
{
    [Preserve]
    public class GraphicsViewRenderer : ViewRenderer<GraphicsView, NativeGraphicsView>
    {
        protected override void OnElementChanged(ElementChangedEventArgs<GraphicsView> e)
        {
            base.OnElementChanged(e);

            if (e.OldElement != null)
            {
                // Unsubscribe from event handlers and cleanup any resources
                SetNativeControl(null);
            }

            if (e.NewElement != null)
            {
                SetNativeControl(new NativeGraphicsView());
            }
        }

        protected override void OnElementPropertyChanged(
            object sender,
            PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            if (e.PropertyName == nameof(GraphicsView.Drawable))
                UpdateDrawable();
        }

        private void UpdateDrawable()
        {
            Control.Drawable = Element.Drawable;
        }
    }
}