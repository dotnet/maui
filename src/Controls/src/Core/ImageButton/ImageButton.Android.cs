using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific;

namespace Microsoft.Maui.Controls
{	
	public partial class ImageButton
	{
		internal static void MapRippleColor(IImageButtonHandler handler, ImageButton imageButton)
		{
			if(imageButton.IsSet(PlatformConfiguration.AndroidSpecific.ImageButton.RippleColorProperty))
			{
				var color = imageButton.OnThisPlatform().GetRippleColor();
				handler.PlatformView?.UpdateRippleColor(color);
			}
		}
	}
}
