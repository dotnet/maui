using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Compatibility.Platform.iOS;

[assembly: ExportEffect(typeof(Microsoft.Maui.Controls.Compatibility.ControlGallery.iOS.CustomEffects.FooEffect), nameof(Microsoft.Maui.Controls.Compatibility.ControlGallery.iOS.CustomEffects.FooEffect))]

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.iOS.CustomEffects
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