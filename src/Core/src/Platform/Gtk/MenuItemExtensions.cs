using System.Collections.Generic;
using Gtk;

namespace Microsoft.Maui.Platform;

public static class MenuItemExtensions
{
	[MissingMapper]
	public static void UpdateKeyboardAccelerators(this MenuItem platformView, IReadOnlyList<IKeyboardAccelerator>? viewKeyboardAccelerators)
	{
	}

	[MissingMapper]
	public static void UpdateImageSource(this MenuItem platformView, IImageSource? viewSource)
	{
	}
	
	public static void UpdateText(this MenuItem platformView, string text)
	{
		platformView.Label = text;
	}
}