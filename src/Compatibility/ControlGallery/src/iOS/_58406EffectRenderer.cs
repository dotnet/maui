using System.Linq;
using UIKit;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls.Compatibility.Platform.iOS;

[assembly: ExportEffect(typeof(Microsoft.Maui.Controls.Compatibility.ControlGallery.iOS._58406EffectRenderer), Bugzilla58406.EffectName)]

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.iOS
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