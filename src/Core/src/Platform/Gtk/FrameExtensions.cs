using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Platform.Gtk;

namespace Microsoft.Maui.Platform;

public static class FrameExtensions
{
	public static void UpdateMapCornerRadius(this FrameView platformView, float cornerRadius)
	{
		if (cornerRadius > 0)
			platformView.SetStyleValueNode($"{(int)cornerRadius}px", platformView.CssMainNode(), "border-radius");
	}

	public static void UpdateBorderColor(this FrameView platformView, Color color)
	{
		platformView.SetStyleValueNode($"1px solid {color.ToGdkRgba().ToString()}", platformView.CssMainNode(), "border");
	}
}