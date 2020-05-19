using global::Windows.UI.Xaml.Controls;
using global::Windows.UI.Xaml.Media;
using System.Maui;
using System.Maui.ControlGallery.WindowsUniversal;
using System.Maui.Platform.UWP;

[assembly: ExportEffect(typeof(BorderEffect), "BorderEffect")]
namespace System.Maui.ControlGallery.WindowsUniversal
{
	public class BorderEffect : PlatformEffect
	{
		protected override void OnAttached()
		{
			var control = Control as Control;
			if (control == null)
				return;

			control.Background = new SolidColorBrush(global::Windows.UI.Colors.Aqua);

			var childLabel = (Element as ScrollView)?.Content as Label;
			if (childLabel != null)
				childLabel.Text = "Success";
		}

		protected override void OnDetached()
		{
			var control = Control as Control;
			if (control == null)
				return;

			control.Background = new SolidColorBrush(global::Windows.UI.Colors.Beige);
		}
	}
}