using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;

namespace Microsoft.Maui.DeviceTests
{
	public partial class ContentViewTests
	{
		static int GetChildCount(ContentViewHandler contentViewHandler)
		{
			return contentViewHandler.PlatformView.ChildCount;
		}

		static int GetContentChildCount(ContentViewHandler contentViewHandler)
		{
			return (contentViewHandler.PlatformView.GetChildAt(0) as LayoutViewGroup).ChildCount;
		}
	}
}
