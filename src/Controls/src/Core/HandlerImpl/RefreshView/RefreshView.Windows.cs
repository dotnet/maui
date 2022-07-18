namespace Microsoft.Maui.Controls
{
	public partial class RefreshView
	{
		public static void MapRefreshPullDirection(RefreshViewHandler handler, RefreshView refreshView) =>
			Platform.RefreshViewExtensions.UpdateRefreshPullDirection(handler.PlatformView, refreshView);
	}
}