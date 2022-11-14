using System.Threading.Tasks;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.DeviceTests
{
	public partial class RefreshViewHandlerTests
	{
		MauiSwipeRefreshLayout GetNativeRefreshView(RefreshViewHandler RefreshViewHandler) =>
			(MauiSwipeRefreshLayout)RefreshViewHandler.PlatformView;

		bool GetPlatformIsRefreshing(RefreshViewHandler RefreshViewHandler) =>
			GetNativeRefreshView(RefreshViewHandler).Refreshing;

		[Fact(DisplayName = "Control meets basic accessibility requirements")]
		[Category(TestCategory.Accessibility)]
		public async Task PlatformViewIsAccessible()
		{
			var view = new RefreshViewStub();
			await AssertPlatformViewIsAccessible(view);
		}
	}
}