#nullable disable
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific;

namespace Microsoft.Maui.Controls
{	
	public partial class ImageButton
	{
		#pragma warning disable RS0016
		public static void MapRippleColor(IImageButtonHandler handler, ImageButton imageButton)
		{
			if(imageButton.IsSet(PlatformConfiguration.AndroidSpecific.ImageButton.RippleColorProperty))
			{
				var color = imageButton.OnThisPlatform().RippleColor();
				handler.PlatformView?.UpdateRippleColor(color);
			}
		}
	}
}
