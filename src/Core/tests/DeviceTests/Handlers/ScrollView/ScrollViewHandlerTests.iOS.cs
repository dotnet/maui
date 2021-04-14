using Microsoft.Maui.Handlers;
using UIKit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class ScrollViewHandlerTests
	{
		UIScrollView GetNativeScrollView(ScrollViewHandler scrollViewHandler) =>
			scrollViewHandler.NativeView;

		object GetNativeContent(ScrollViewHandler scrollViewHandler)
		{
			var nativeScrollView = GetNativeScrollView(scrollViewHandler);
			return nativeScrollView.Subviews[0];
		}
	}
}