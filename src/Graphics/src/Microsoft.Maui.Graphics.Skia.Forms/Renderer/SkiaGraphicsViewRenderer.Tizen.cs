using System.ComponentModel;
using Microsoft.Maui.Graphics.Forms;
using Xamarin.Forms;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Platform.Tizen;
using XForms = Xamarin.Forms.Forms;

[assembly: ExportRenderer(typeof(SkiaGraphicsView), typeof(Microsoft.Maui.Graphics.Skia.Views.SkiaGraphicsView))]
namespace Microsoft.Maui.Graphics.Skia.Forms
{
    [Preserve]
    public class SkiaGraphicsViewRenderer : ViewRenderer<SkiaGraphicsView, Microsoft.Maui.Graphics.Skia.Views.SkiaGraphicsView>
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
                SetNativeControl(new Microsoft.Maui.Graphics.Skia.Views.SkiaGraphicsView(XForms.NativeParent));
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