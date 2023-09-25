using System;
using System.Linq;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Compatibility.Platform.Android;
using Microsoft.Maui.Controls.ControlGallery.Android;
using Microsoft.Maui.Controls.ControlGallery.Effects;
using Microsoft.Maui.Controls.Platform;

[assembly: ExportEffect(typeof(AttachedStateEffectRenderer), AttachedStateEffect.EffectName)]
namespace Microsoft.Maui.Controls.ControlGallery.Android
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