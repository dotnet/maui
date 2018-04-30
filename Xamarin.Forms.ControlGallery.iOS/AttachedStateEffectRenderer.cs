using System;
using System.Linq;
using Xamarin.Forms;
using Xamarin.Forms.ControlGallery.iOS;
using Xamarin.Forms.Controls.Effects;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportEffect(typeof(AttachedStateEffectRenderer), AttachedStateEffect.EffectName)]
namespace Xamarin.Forms.ControlGallery.iOS
{
	public class AttachedStateEffectRenderer : PlatformEffect
	{
		public AttachedStateEffect MyEffect { get; private set; }

		protected override void OnAttached()
		{
			MyEffect = Element.Effects.OfType<AttachedStateEffect>().First();
			MyEffect.Attached(Element);

		}

		protected override void OnDetached()
		{
			MyEffect.Detached(Element);
		}
	}
}