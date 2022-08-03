using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;

namespace Microsoft.Maui.DeviceTests
{
	public partial class ContentViewTests
	{
		static int GetChildCount(ContentViewHandler contentViewHandler)
		{
			return contentViewHandler.PlatformView.Children.Count;
		}

		static int GetContentChildCount(ContentViewHandler contentViewHandler)
		{
			if (contentViewHandler.PlatformView.Children[0] is LayoutPanel childLayoutPanel)
				return childLayoutPanel.Children.Count;
			else
				return 0;
		}
	}
}
