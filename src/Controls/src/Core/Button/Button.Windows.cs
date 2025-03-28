#nullable disable
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Platform;

namespace Microsoft.Maui.Controls
{
	public partial class Button
	{
		public static void MapImageSource(ButtonHandler handler, Button button) =>
			MapImageSource((IButtonHandler)handler, button);

		public static void MapText(IButtonHandler handler, Button button)
		{
			var text = TextTransformUtilities.GetTransformedText(button.Text, button.TextTransform);
			handler.PlatformView?.UpdateText(text);
			button.Handler?.UpdateValue(nameof(Button.ContentLayout));
		}

		public static void MapLineBreakMode(IButtonHandler handler, Button button)
		{
			handler.PlatformView?.UpdateLineBreakMode(button);
		}

		public static void MapImageSource(IButtonHandler handler, Button button)
		{
			ButtonHandler.MapImageSource(handler, button);
			button.Handler?.UpdateValue(nameof(Button.ContentLayout));
		}

		public static void MapText(ButtonHandler handler, Button button) =>
			MapText((IButtonHandler)handler, button);
	}
}