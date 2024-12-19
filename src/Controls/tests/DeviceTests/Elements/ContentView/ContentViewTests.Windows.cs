using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;

namespace Microsoft.Maui.DeviceTests
{
	public partial class ContentViewTests
	{
		static int GetChildCount(ContentViewHandler contentViewHandler)
		{
			return contentViewHandler.PlatformView.CachedChildren.Count;
		}

		static int GetContentChildCount(ContentViewHandler contentViewHandler)
		{
			if (contentViewHandler.PlatformView.CachedChildren[0] is LayoutPanel childLayoutPanel)
				return childLayoutPanel.CachedChildren.Count;
			else
				return 0;
		}
	}
}
