using System.Maui.Platform.Android;

namespace System.Maui.ControlGallery.Android
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