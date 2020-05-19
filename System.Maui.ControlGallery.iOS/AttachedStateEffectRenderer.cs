using System;
using System.Linq;
using System.Maui;
using System.Maui.ControlGallery.iOS;
using System.Maui.Controls.Effects;
using System.Maui.Platform.iOS;

[assembly: ExportEffect(typeof(AttachedStateEffectRenderer), AttachedStateEffect.EffectName)]
namespace System.Maui.ControlGallery.iOS
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