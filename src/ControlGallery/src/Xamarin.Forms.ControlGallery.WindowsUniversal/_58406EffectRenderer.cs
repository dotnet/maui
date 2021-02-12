using System.Linq;
using Microsoft.UI.Xaml.Controls;
using Xamarin.Forms;
using Xamarin.Forms.Controls.Issues;
using Xamarin.Forms.Platform.UWP;

[assembly: ExportEffect(typeof(Xamarin.Forms.ControlGallery.WindowsUniversal._58406EffectRenderer), Bugzilla58406.EffectName)]

namespace Xamarin.Forms.ControlGallery.WindowsUniversal
{
	public class _58406EffectRenderer : PlatformEffect
	{
		protected override void OnAttached()
		{
			var textBlock = Control as TextBlock;

			if (textBlock == null)
			{
				return;
			}

			textBlock.Text = new string(textBlock.Text.ToCharArray().Reverse().ToArray()); 
		}

		protected override void OnDetached()
		{
		}
	}
}