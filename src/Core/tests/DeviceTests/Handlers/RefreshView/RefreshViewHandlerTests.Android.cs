using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.DeviceTests
{
	public partial class RefreshViewHandlerTests
	{
		MauiSwipeRefreshLayout GetNativeRefreshView(RefreshViewHandler RefreshViewHandler) =>
			(MauiSwipeRefreshLayout)RefreshViewHandler.PlatformView;

		bool GetNativeIsRefreshing(RefreshViewHandler RefreshViewHandler) =>
			GetNativeRefreshView(RefreshViewHandler).Refreshing;
	}
}