using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.DeviceTests
{
	public partial class ContentViewTests
	{
		static int GetChildCount(ContentViewHandler contentViewHandler)
		{
			return contentViewHandler.PlatformView.Subviews.Length;
		}

		static int GetContentChildCount(ContentViewHandler contentViewHandler)
		{
			return contentViewHandler.PlatformView.Subviews[0].Subviews.Length;
		}
	}
}
