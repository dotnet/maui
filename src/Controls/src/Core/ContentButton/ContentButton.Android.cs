using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;

namespace Microsoft.Maui.Controls
{
	public partial class ContentButton
	{
		public static void MapRippleColor(IContentButtonHandler handler, ContentButton button)
		{
			if (handler.PlatformView is MauiContentButton platformView)
				platformView.UpdateRippleColor(button.OnThisPlatform().GetRippleColor());
		}
	}
}
