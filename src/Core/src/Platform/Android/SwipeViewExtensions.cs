using Microsoft.Maui.Graphics;
using AColor = Android.Graphics.Color;

namespace Microsoft.Maui.Platform
{
	public static class SwipeViewExtensions
	{
		public static AColor? GetTextColor(this ISwipeItemMenuItem swipeItemMenuItem)
		{
			Color? backgroundColor = swipeItemMenuItem.Background?.ToColor();

			if (backgroundColor == null)
				return null;

			var luminosity = 0.2126f * backgroundColor.Red + 0.7152f * backgroundColor.Green + 0.0722f * backgroundColor.Blue;

			return (luminosity < 0.75f ? Colors.White : Colors.Black).ToNative();
		}
	}
}
