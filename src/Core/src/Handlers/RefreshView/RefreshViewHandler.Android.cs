using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Handlers
{
	public partial class RefreshViewHandler : ViewHandler<IRefreshView, MauiSwipeRefreshLayout>
	{
		protected override MauiSwipeRefreshLayout CreatePlatformView()
		{
			return new MauiSwipeRefreshLayout(Context);
		}

		public override void SetVirtualView(IView view)
		{
			base.SetVirtualView(view);
			PlatformView.CrossPlatformLayout = VirtualView as ICrossPlatformLayout;
		}

		protected override void ConnectHandler(MauiSwipeRefreshLayout platformView)
		{
			base.ConnectHandler(platformView);
			platformView.Refresh += OnSwipeRefresh;
		}

		void OnSwipeRefresh(object? sender, System.EventArgs e)
		{
			VirtualView.IsRefreshing = true;
		}

		protected override void DisconnectHandler(MauiSwipeRefreshLayout platformView)
		{
			// If we're being disconnected from the xplat element, then we should no longer be managing its children
			platformView.Refresh -= OnSwipeRefresh;
			platformView.UpdateContent(null, null);
			base.DisconnectHandler(platformView);
		}

		static void UpdateContent(IRefreshViewHandler handler) =>
			handler.PlatformView.UpdateContent(handler.VirtualView.Content, handler.MauiContext);

		static void UpdateRefreshColor(IRefreshViewHandler handler)
		{
			if (handler.VirtualView.RefreshColor == null)
				return;

			var color = handler.VirtualView.RefreshColor.ToColor()?.ToInt();

			if (color != null)
				handler.PlatformView.SetColorSchemeColors(color.Value);
		}

		static void UpdateBackground(IRefreshViewHandler handler)
		{
			if (handler.VirtualView.Background == null)
				return;

			var color = handler.VirtualView.Background.ToColor()?.ToInt();
			if (color != null)
				handler.PlatformView.SetProgressBackgroundColorSchemeColor(color.Value);
		}

		public static void MapBackground(IRefreshViewHandler handler, IView view)
			=> UpdateBackground(handler);

		public static void MapIsRefreshing(IRefreshViewHandler handler, IRefreshView refreshView)
			=> handler.PlatformView.Refreshing = handler.VirtualView.IsRefreshing;

		public static void MapContent(IRefreshViewHandler handler, IRefreshView refreshView)
			=> UpdateContent(handler);

		public static void MapRefreshColor(IRefreshViewHandler handler, IRefreshView refreshView)
			=> UpdateRefreshColor(handler);

		internal static void MapIsRefreshEnabled(IRefreshViewHandler handler, IRefreshView refreshView)
			=> handler.PlatformView.RefreshEnabled = handler.VirtualView.IsRefreshEnabled;

		public static void MapIsEnabled(IRefreshViewHandler handler, IRefreshView refreshView)
			=> handler.PlatformView.Enabled = handler.VirtualView.IsEnabled;
	}
}
