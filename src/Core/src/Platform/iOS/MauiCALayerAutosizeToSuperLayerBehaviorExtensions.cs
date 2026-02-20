using System;
using CoreAnimation;

namespace Microsoft.Maui.Platform;

internal static class MauiCALayerAutosizeToSuperLayerBehaviorExtensions
{
	public static void AttachOrThrow(this MauiCALayerAutosizeToSuperLayerBehavior behavior, CALayer layer)
	{
		var result = behavior.Attach(layer);

		if (result != MauiCALayerAutosizeToSuperLayerResult.Success)
		{
			throw new InvalidOperationException($"Failed to attach MauiCALayerAutosizeToSuperLayerBehavior: {result}");
		}
	}
}