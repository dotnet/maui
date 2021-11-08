using System;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Handlers
{
	public partial class RefreshViewHandler : ViewHandler<IRefreshView, MauiRefreshView>
	{
		protected override MauiRefreshView CreateNativeView()
		{
			return new MauiRefreshView();
		}

		protected override void ConnectHandler(MauiRefreshView nativeView)
		{
			nativeView.RefreshControl.ValueChanged += OnRefresh;

			base.ConnectHandler(nativeView);
		}

		protected override void DisconnectHandler(MauiRefreshView nativeView)
		{
			nativeView.RefreshControl.ValueChanged -= OnRefresh;

			base.DisconnectHandler(nativeView);
		}

		public static void MapBackground(RefreshViewHandler handler, IRefreshView view)
			=> handler.NativeView.RefreshControl.UpdateBackground(view);

		public static void MapIsRefreshing(RefreshViewHandler handler, IRefreshView refreshView)
			=> handler.UpdateIsRefreshing();

		public static void MapContent(RefreshViewHandler handler, IRefreshView refreshView)
			=> handler.UpdateContent();

		public static void MapRefreshColor(RefreshViewHandler handler, IRefreshView refreshView)
			=> handler.UpdateRefreshColor();

		public static void MapIsEnabled(RefreshViewHandler handler, IRefreshView refreshView)
			=> handler.NativeView?.UpdateIsEnabled(refreshView.IsEnabled);

		void OnRefresh(object? sender, EventArgs e)
		{
			VirtualView.IsRefreshing = true;
		}

		void UpdateIsRefreshing()
		{
			NativeView.IsRefreshing = VirtualView.IsRefreshing;
		}

		void UpdateContent() =>
			NativeView.UpdateContent(VirtualView.Content, MauiContext);

		void UpdateRefreshColor()
		{
			var color = VirtualView?.RefreshColor?.ToColor()?.ToNative();

			if (color != null)
				NativeView.RefreshControl.TintColor = color;
		}
	}
}
