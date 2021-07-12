using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Controls.Platform;

namespace Microsoft.Maui.Controls.Hosting
{
	public interface IEffectsBuilder
	{
		IEffectsBuilder Add<TEffect, TPlatformEffect>()
			where TEffect : RoutingEffect
			where TPlatformEffect : PlatformEffect, new();

		IEffectsBuilder Add(Type TEffect, Type TPlatformEffect);
	}
}
