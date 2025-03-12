using System;
using System.Collections.Generic;
using System.Threading;

#if !NET9_0_OR_GREATER
using Lock = object;
#endif

namespace Microsoft.Maui.Handlers;

internal static class RemappingHelper
{
	private static readonly HashSet<Type> s_remappedTypes = new();
	private static readonly Lock s_lock = new();

	public static void RemapIfNeeded(Type handlerType, IElementHandler handler)
	{
		if (ShouldRemap(handlerType))
		{
			(handler as IRemappable)?.RemapForControls();
		}
	}

	public static void RemapIfNeeded(Type handlerType, Action remapForControls)
	{
		if (ShouldRemap(handlerType))
		{
			remapForControls();
		}
	}

	private static bool ShouldRemap(Type handlerType)
	{
		lock (s_lock)
		{
			return s_remappedTypes.Add(handlerType);
		}
	}
}
