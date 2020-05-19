using System.Linq;
using UIKit;
using System.Maui;
using System.Maui.Controls.Issues;
using System.Maui.Platform.iOS;

[assembly: ExportEffect(typeof(System.Maui.ControlGallery.iOS._58406EffectRenderer), Bugzilla58406.EffectName)]

namespace System.Maui.ControlGallery.iOS
{
	public class _58406EffectRenderer : PlatformEffect
	{
		protected override void OnAttached()
		{
			var tv = Control as UILabel;

			if (tv == null)
			{
				return;
			}

			tv.Text = new string(tv.Text.ToCharArray().Reverse().ToArray()); 
		}

		protected override void OnDetached()
		{
		}
	}
}