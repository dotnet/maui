using System.ComponentModel;
using System.Graphics.Android;
using System.Graphics.Forms.Android;
using Android.Content;
using Xamarin.Forms;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(System.Graphics.Forms.SkiaGraphicsView), typeof(SkiaGraphicsViewRenderer))]
namespace System.Graphics.Forms.Android
{
    [Preserve]
    public class SkiaGraphicsViewRenderer : ViewRenderer<System.Graphics.Forms.SkiaGraphicsView, NativeGraphicsView>
    {
        public SkiaGraphicsViewRenderer(Context context) : base(context)
        {

        }

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
                SetNativeControl(new NativeGraphicsView(Context));
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