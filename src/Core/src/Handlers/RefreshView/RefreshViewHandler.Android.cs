using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Handlers
{
	public partial class RefreshViewHandler : ViewHandler<IRefreshView, MauiSwipeRefreshLayout>
	{
		protected override MauiSwipeRefreshLayout CreatePlatformView()
		{
			return new MauiSwipeRefreshLayout(Context);
		}

		protected override void ConnectHandler(MauiSwipeRefreshLayout nativeView)
		{
			base.ConnectHandler(nativeView);
			nativeView.Refresh += OnSwipeRefresh;
		}

		void OnSwipeRefresh(object? sender, System.EventArgs e)
		{
			VirtualView.IsRefreshing = true;
		}

		protected override void DisconnectHandler(MauiSwipeRefreshLayout nativeView)
		{
			// If we're being disconnected from the xplat element, then we should no longer be managing its chidren
			nativeView.Refresh -= OnSwipeRefresh;
			nativeView.UpdateContent(null, null);
			base.DisconnectHandler(nativeView);
		}

		void UpdateContent() =>
			PlatformView.UpdateContent(VirtualView.Content, MauiContext);

		void UpdateRefreshColor()
		{
			if (VirtualView.RefreshColor == null)
				return;

			var color = VirtualView.RefreshColor.ToColor()?.ToInt();

			if (color != null)
				PlatformView.SetColorSchemeColors(color.Value);
		}

		void UpdateIsRefreshing() =>
			PlatformView.Refreshing = VirtualView.IsRefreshing;

		void UpdateBackground()
		{
			if (VirtualView.Background == null)
				return;

			var color = VirtualView.Background.ToColor()?.ToInt();
			if (color != null)
				PlatformView.SetProgressBackgroundColorSchemeColor(color.Value);
		}

		public static void MapBackground(RefreshViewHandler handler, IView view)
			=> handler.UpdateBackground();

		public static void MapIsRefreshing(RefreshViewHandler handler, IRefreshView refreshView)
			=> handler.UpdateIsRefreshing();

		public static void MapContent(RefreshViewHandler handler, IRefreshView refreshView)
			=> handler.UpdateContent();

		public static void MapRefreshColor(RefreshViewHandler handler, IRefreshView refreshView)
			=> handler.UpdateRefreshColor();
	}
}
