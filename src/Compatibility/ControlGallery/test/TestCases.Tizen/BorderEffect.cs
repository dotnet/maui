using System.ComponentModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Compatibility.ControlGallery.Tizen;
using Microsoft.Maui.Controls.Compatibility.Platform.Tizen;
using Microsoft.Maui.Controls.Platform;
using ElmSharp;
using EColor = ElmSharp.Color;

[assembly: ExportEffect(typeof(BorderEffect), "BorderEffect")]
namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Tizen
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