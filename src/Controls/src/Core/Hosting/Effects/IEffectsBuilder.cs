using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Controls.Platform;

namespace Microsoft.Maui.Controls.Hosting
{
	public interface IEffectsBuilder
	{
		void Add<TEffect, TPlatformEffect>()
			where TEffect : RoutingEffect
			where TPlatformEffect : PlatformEffect, new();

		void Add(Type TEffect, Type TPlatformEffect);
	}
}
