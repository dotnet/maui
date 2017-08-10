using System.Linq;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Controls.Issues;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportEffect(typeof(Xamarin.Forms.ControlGallery.iOS._58406EffectRenderer), Bugzilla58406.EffectName)]

namespace Xamarin.Forms.ControlGallery.iOS
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