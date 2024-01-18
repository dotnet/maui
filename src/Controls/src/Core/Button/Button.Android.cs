#nullable disable
using Google.Android.Material.Button;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	public partial class Button
	{
		public static void MapText(IButtonHandler handler, Button button)
		{
			handler.PlatformView?.UpdateText(button);
		}

		public static void MapText(ButtonHandler handler, Button button) =>
			MapText((IButtonHandler)handler, button);

		public static void MapLineBreakMode(IButtonHandler handler, Button button)
		{
			handler.PlatformView?.UpdateLineBreakMode(button);
		}

		protected override Size ArrangeOverride(Rect bounds)
		{
			Handler?.UpdateValue(nameof(ContentLayout));

			return base.ArrangeOverride(bounds);
		}
	}
}