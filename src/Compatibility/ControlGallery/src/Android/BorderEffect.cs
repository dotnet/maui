using Microsoft.Maui.Controls.Compatibility.Platform.Android;
using Microsoft.Maui.Controls.Platform;

namespace Microsoft.Maui.Controls.ControlGallery.Android
{
	public class BorderEffect : PlatformEffect
	{
		protected override void OnAttached()
		{
			Control.SetBackgroundColor(global::Android.Graphics.Color.Aqua);

			var childLabel = (Element as ScrollView)?.Content as Label;
			if (childLabel != null)
				childLabel.Text = "Success";
		}

		protected override void OnDetached()
		{
			Control.SetBackgroundColor(global::Android.Graphics.Color.Beige);
		}
	}
}