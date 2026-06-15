using System;
using System.Collections.Generic;

namespace Microsoft.Maui.Handlers;

internal interface IControlsMapperRemappable
{
	void RemapForControls(HashSet<Type> remapped);
}

internal static class ControlsMapperRemapper
{
	static readonly object s_remappedForControlsLock = new();
	static readonly HashSet<Type> s_remappedForControls = new();

	internal static void EnsureRemapped(IControlsMapperRemappable remappable)
	{
		lock (s_remappedForControlsLock)
		{
			remappable.RemapForControls(s_remappedForControls);
		}
	}
}
