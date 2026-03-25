using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific;

namespace Microsoft.Maui.Controls
{
	public partial class ImageButton
	{
		public static void MapRippleColor(IImageButtonHandler handler, ImageButton imageButton)
		{
			var color = imageButton.OnThisPlatform().GetRippleColor();
			handler.PlatformView?.UpdateRippleColor(color);
		}
	}
}
