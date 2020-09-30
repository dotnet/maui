using Xamarin.Forms.Platform.Android;

namespace Xamarin.Forms.ControlGallery.Android
{
    public class BorderEffect : PlatformEffect
    {
        protected override void OnAttached ()
        {
            Control.SetBackgroundColor (global::Android.Graphics.Color.Aqua);

            var childLabel = (Element as ScrollView)?.Content as Label;
            if (childLabel != null)
                childLabel.Text = "Success";
        }

        protected override void OnDetached ()
        {
            Control.SetBackgroundColor(global::Android.Graphics.Color.Beige);
        }
    }
}