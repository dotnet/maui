using System.Linq;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Compatibility.Platform.UWP;
using Microsoft.Maui.Controls.ControlGallery.Issues;
using Microsoft.Maui.Controls.Platform;
using Microsoft.UI.Xaml.Controls;

[assembly: ExportEffect(typeof(Microsoft.Maui.Controls.ControlGallery.WinUI._58406EffectRenderer), Bugzilla58406.EffectName)]

namespace Microsoft.Maui.Controls.ControlGallery.WinUI
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