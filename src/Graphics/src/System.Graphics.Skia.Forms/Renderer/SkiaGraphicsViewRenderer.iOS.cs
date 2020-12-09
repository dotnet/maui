using System.ComponentModel;
using System.Graphics.CoreGraphics;
using System.Graphics.Forms;
using System.Graphics.Forms.iOS;
using Foundation;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(SkiaGraphicsView), typeof(SkiaGraphicsViewRenderer))]
namespace System.Graphics.Forms.iOS
{
    [Preserve]
    public class SkiaGraphicsViewRenderer : ViewRenderer<SkiaGraphicsView, NativeGraphicsView>
    {
        protected override void OnElementChanged(ElementChangedEventArgs<SkiaGraphicsView> e)
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

            if (e.PropertyName == nameof(SkiaGraphicsView.Drawable))
                UpdateDrawable();
        }

        private void UpdateDrawable()
        {
            Control.Drawable = Element.Drawable;
        }
    }
}