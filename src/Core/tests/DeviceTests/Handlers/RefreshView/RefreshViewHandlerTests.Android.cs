using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.DeviceTests
{
	public partial class RefreshViewHandlerTests
	{
		MauiSwipeRefreshLayout GetNativeRefreshView(RefreshViewHandler RefreshViewHandler) =>
			(MauiSwipeRefreshLayout)RefreshViewHandler.PlatformView;

		bool GetPlatformIsRefreshing(RefreshViewHandler RefreshViewHandler) =>
			GetNativeRefreshView(RefreshViewHandler).Refreshing;
	}
}