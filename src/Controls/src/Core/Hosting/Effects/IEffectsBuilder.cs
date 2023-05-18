#nullable disable
using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Maui.Controls.Platform;

namespace Microsoft.Maui.Controls.Hosting
{
	public interface IEffectsBuilder
	{
		IEffectsBuilder Add<TEffect, TPlatformEffect>()
			where TEffect : RoutingEffect
			where TPlatformEffect : PlatformEffect, new();

		IEffectsBuilder Add(Type TEffect, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors)] Type TPlatformEffect);
	}
}
