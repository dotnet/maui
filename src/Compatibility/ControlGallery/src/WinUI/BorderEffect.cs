using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Xamarin.Forms;
using Microsoft.Maui.Controls.Compatibility.ControlGallery.Windows;
using Microsoft.Maui.Controls.Compatibility.Platform.UWP;
using Microsoft.Maui.Controls;

[assembly: ExportEffect(typeof(BorderEffect), "BorderEffect")]
namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Windows
{
	public class BorderEffect : PlatformEffect
	{
		protected override void OnAttached()
		{
			var control = Control as Control;
			if (control == null)
				return;

			control.Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Aqua);

			var childLabel = (Element as ScrollView)?.Content as Label;
			if (childLabel != null)
				childLabel.Text = "Success";
		}

		protected override void OnDetached()
		{
			var control = Control as Control;
			if (control == null)
				return;

			control.Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Beige);
		}
	}
}