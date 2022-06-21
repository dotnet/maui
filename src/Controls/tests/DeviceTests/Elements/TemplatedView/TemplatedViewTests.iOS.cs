using Microsoft.Maui.Handlers;
using UIKit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class TemplatedViewTests
	{
		static int GetChildCount(ContentViewHandler contentViewHandler) =>
			contentViewHandler.PlatformView.Subviews.Length;

		static UIView GetChild(ContentViewHandler contentViewHandler, int index = 0) =>
			contentViewHandler.PlatformView.Subviews[index];
	}
}
