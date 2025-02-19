using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Compatibility.Platform.iOS;
using Microsoft.Maui.Controls.Platform;

[assembly: ExportEffect(typeof(Microsoft.Maui.Controls.ControlGallery.iOS.CustomEffects.FooEffect), nameof(Microsoft.Maui.Controls.ControlGallery.iOS.CustomEffects.FooEffect))]

namespace Microsoft.Maui.Controls.ControlGallery.iOS.CustomEffects
{
	public class FooEffect : PlatformEffect
	{
		public FooEffect()
		{
		}

		protected override void OnAttached()
		{
		}

		protected override void OnDetached()
		{

		}
	}
}