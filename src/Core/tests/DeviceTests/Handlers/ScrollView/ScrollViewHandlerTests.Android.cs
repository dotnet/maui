using AndroidX.Core.Widget;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.DeviceTests
{
	public partial class ScrollViewHandlerTests
	{
		NestedScrollView GetNativeScrollView(ScrollViewHandler scrollViewHandler) =>
			scrollViewHandler.NativeView;

		object GetNativeContent(ScrollViewHandler scrollViewHandler)
		{
			var nativeScrollView = GetNativeScrollView(scrollViewHandler);

			if (nativeScrollView.GetChildAt(0) is ScrollViewContainer scrollViewContainer)
				return scrollViewContainer.ChildView;

			return null;
		}
	}
}