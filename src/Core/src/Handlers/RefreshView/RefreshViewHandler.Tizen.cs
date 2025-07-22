using System;

namespace Microsoft.Maui.Handlers
{
	public partial class RefreshViewHandler : ViewHandler<IRefreshView, MauiRefreshLayout>
	{

		protected override MauiRefreshLayout CreatePlatformView() => new();

		protected override void ConnectHandler(MauiRefreshLayout platformView)
		{
			base.ConnectHandler(platformView);
			platformView.Refreshing += OnRefreshing;
		}

		void OnRefreshing(object? sender, EventArgs e)
		{
			VirtualView.IsRefreshing = true;
		}

		protected override void DisconnectHandler(MauiRefreshLayout platformView)
		{
			platformView.Refreshing -= OnRefreshing;
			platformView.Content = null;
			base.DisconnectHandler(platformView);
		}

		public static void MapIsRefreshing(IRefreshViewHandler handler, IRefreshView refreshView) =>
			handler.PlatformView.UpdateIsRefreshing(refreshView);

		public static void MapContent(IRefreshViewHandler handler, IRefreshView refreshView) =>
			handler.PlatformView.UpdateContent(handler.VirtualView.Content, handler.MauiContext);

		public static void MapRefreshColor(IRefreshViewHandler handler, IRefreshView refreshView) =>
			handler.PlatformView.UpdateRefreshColor(refreshView);

		public static void MapBackground(IRefreshViewHandler handler, IRefreshView view)
			=> handler.PlatformView.UpdateBackground(view);
	}
}
