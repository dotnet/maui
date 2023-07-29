using System;

namespace Microsoft.Maui.Controls;

#pragma warning disable RS0016
public static class PointerEventArgsExtensions
{
	public static PointerPlatformEventArgs? ToPlatform(this PointerEventArgs eventArgs)
	{
		return eventArgs.Args;
	}
}
