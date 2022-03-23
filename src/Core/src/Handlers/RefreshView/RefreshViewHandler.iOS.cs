using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Graphics;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Handlers
{
	public partial class RefreshViewHandler : ViewHandler<IRefreshView, UIView>
	{
		MauiRefreshView? GetMauiRefreshView() => PlatformView as MauiRefreshView;
		static MauiRefreshView? GetMauiRefreshView(IRefreshViewHandler handler) => handler.PlatformView as MauiRefreshView;

		protected override UIView CreatePlatformView()
		{
			return new MauiRefreshView();
		}

		protected override void ConnectHandler(UIView platformView)
		{
			if (platformView is MauiRefreshView refreshView)
				refreshView.RefreshControl.ValueChanged += OnRefresh;

			base.ConnectHandler(platformView);
		}

		protected override void DisconnectHandler(UIView platformView)
		{
			if (platformView is MauiRefreshView refreshView)
				refreshView.RefreshControl.ValueChanged -= OnRefresh;

			base.DisconnectHandler(platformView);
		}

		public static void MapBackground(IRefreshViewHandler handler, IRefreshView view)
			=> GetMauiRefreshView(handler)?.RefreshControl?.UpdateBackground(view);

		public static void MapIsRefreshing(IRefreshViewHandler handler, IRefreshView refreshView)
			=> UpdateIsRefreshing(handler);

		public static void MapContent(IRefreshViewHandler handler, IRefreshView refreshView)
			=> UpdateContent(handler);

		public static void MapRefreshColor(IRefreshViewHandler handler, IRefreshView refreshView)
			=> UpdateRefreshColor(handler);

		public static void MapIsEnabled(IRefreshViewHandler handler, IRefreshView refreshView)
			=> GetMauiRefreshView(handler)?.UpdateIsEnabled(refreshView.IsEnabled);

		void OnRefresh(object? sender, EventArgs e)
		{
			VirtualView.IsRefreshing = true;
		}

		static void UpdateIsRefreshing(IRefreshViewHandler handler)
		{
			if (handler.PlatformView is MauiRefreshView refreshView)
				refreshView.IsRefreshing = handler.VirtualView.IsRefreshing;
		}

		static void UpdateContent(IRefreshViewHandler handler) =>
			GetMauiRefreshView(handler)?.UpdateContent(handler.VirtualView.Content, handler.MauiContext);

		static void UpdateRefreshColor(IRefreshViewHandler handler)
		{
			var color = handler.VirtualView?.RefreshColor?.ToColor()?.ToPlatform();

			if (color != null && handler.PlatformView is MauiRefreshView refreshView)
				refreshView.TintColor = color;
		}
	}
}
