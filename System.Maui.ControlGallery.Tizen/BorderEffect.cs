using System.ComponentModel;
using ElmSharp;
using System.Maui;
using System.Maui.ControlGallery.Tizen;
using System.Maui.Platform.Tizen;
using EColor = ElmSharp.Color;

[assembly: ExportEffect(typeof(BorderEffect), "BorderEffect")]
namespace System.Maui.ControlGallery.Tizen
{
	public class BorderEffect : PlatformEffect
	{
		protected override void OnAttached()
		{
			if (Control is Widget)
			{
				(Control as Widget).BackgroundColor = EColor.Aqua;
			}
			var childLabel = (Element as ScrollView)?.Content as Label;
			if (childLabel != null)
				childLabel.Text = "Success";
		}

		protected override void OnDetached()
		{
			if (Control is Widget)
			{
				(Control as Widget).BackgroundColor = EColor.Default;
			}
		}
	}
}