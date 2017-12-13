using System.ComponentModel;
using ElmSharp;
using Xamarin.Forms;
using Xamarin.Forms.ControlGallery.Tizen;
using Xamarin.Forms.Platform.Tizen;
using EColor = ElmSharp.Color;

[assembly: ExportEffect(typeof(BorderEffect), "BorderEffect")]
namespace Xamarin.Forms.ControlGallery.Tizen
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