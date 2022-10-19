namespace Microsoft.Maui.Controls
{
	public partial class RefreshView
	{
		public static void MapRefreshPullDirection(RefreshViewHandler handler, RefreshView refreshView)
			=> MapRefreshPullDirection((IRefreshViewHandler)handler, refreshView);

		public static void MapRefreshPullDirection(IRefreshViewHandler handler, RefreshView refreshView) =>
			Platform.RefreshViewExtensions.UpdateRefreshPullDirection(handler.PlatformView, refreshView);
	}
}