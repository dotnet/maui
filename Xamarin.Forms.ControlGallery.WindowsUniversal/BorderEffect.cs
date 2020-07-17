using Windows.UI.Xaml.Controls;
using Xamarin.Forms;
using Xamarin.Forms.ControlGallery.WindowsUniversal;
using Xamarin.Forms.Platform.UWP;
using WSolidColorBrush = Windows.UI.Xaml.Media.SolidColorBrush;

[assembly: ExportEffect(typeof(BorderEffect), "BorderEffect")]
namespace Xamarin.Forms.ControlGallery.WindowsUniversal
{
	public class BorderEffect : PlatformEffect
	{
		protected override void OnAttached()
		{
			var control = Control as Control;
			if (control == null)
				return;

			control.Background = new WSolidColorBrush(Windows.UI.Colors.Aqua);

			var childLabel = (Element as ScrollView)?.Content as Label;
			if (childLabel != null)
				childLabel.Text = "Success";
		}

		protected override void OnDetached()
		{
			var control = Control as Control;
			if (control == null)
				return;

			control.Background = new WSolidColorBrush(Windows.UI.Colors.Beige);
		}
	}
}