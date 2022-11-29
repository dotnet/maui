using Microsoft.Maui.Handlers;
using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui.DeviceTests
{
	public partial class RefreshViewHandlerTests
	{
		RefreshContainer GetNativeRefreshView(RefreshViewHandler RefreshViewHandler) =>
			RefreshViewHandler.PlatformView;

		bool GetPlatformIsRefreshing(RefreshViewHandler RefreshViewHandler) =>
			GetNativeRefreshView(RefreshViewHandler).Visualizer?.State == RefreshVisualizerState.Refreshing;
	}
}