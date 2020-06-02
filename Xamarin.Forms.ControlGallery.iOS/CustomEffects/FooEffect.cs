using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportEffect(typeof(Xamarin.Forms.ControlGallery.iOS.CustomEffects.FooEffect), nameof(Xamarin.Forms.ControlGallery.iOS.CustomEffects.FooEffect))]

namespace Xamarin.Forms.ControlGallery.iOS.CustomEffects
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