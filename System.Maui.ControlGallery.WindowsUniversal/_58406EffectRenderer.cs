using System.Linq;
using global::Windows.UI.Xaml.Controls;
using System.Maui;
using System.Maui.Controls.Issues;
using System.Maui.Platform.UWP;

[assembly: ExportEffect(typeof(System.Maui.ControlGallery.WindowsUniversal._58406EffectRenderer), Bugzilla58406.EffectName)]

namespace System.Maui.ControlGallery.WindowsUniversal
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