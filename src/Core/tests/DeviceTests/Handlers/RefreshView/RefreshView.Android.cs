using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.DeviceTests
{
	public partial class RefreshViewHandlerTests
	{
		MauiSwipeRefreshLayout GetNativeRefreshView(RefreshViewHandler RefreshViewHandler) =>
			(MauiSwipeRefreshLayout)RefreshViewHandler.NativeView;

		bool GetNativeIsRefreshing(RefreshViewHandler RefreshViewHandler) =>
			GetNativeRefreshView(RefreshViewHandler).Refreshing;
	}
}