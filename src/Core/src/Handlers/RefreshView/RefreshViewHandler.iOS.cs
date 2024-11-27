using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Graphics;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Handlers
{
	public partial class RefreshViewHandler : ViewHandler<IRefreshView, MauiRefreshView>
	{
		readonly MauiRefreshViewProxy _proxy = new();

		protected override MauiRefreshView CreatePlatformView()
		{
			return new MauiRefreshView
			{
				CrossPlatformLayout = VirtualView as ICrossPlatformLayout
			};
		}

		public override void SetVirtualView(IView view)
		{
			base.SetVirtualView(view);

			_ = PlatformView ?? throw new InvalidOperationException($"{nameof(PlatformView)} should have been set by base class.");
			_ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");

			PlatformView.View = view;
			PlatformView.CrossPlatformLayout = VirtualView as ICrossPlatformLayout;
		}

		protected override void ConnectHandler(MauiRefreshView platformView)
		{
			_proxy.Connect(VirtualView, platformView);

			base.ConnectHandler(platformView);
		}

		protected override void DisconnectHandler(MauiRefreshView platformView)
		{
			_proxy.Disconnect(platformView);
			platformView.CrossPlatformLayout = null;
			platformView.RemoveFromSuperview();

			base.DisconnectHandler(platformView);
		}

		public static void MapBackground(IRefreshViewHandler handler, IRefreshView view)
			=> handler.PlatformView.RefreshControl.UpdateBackground(view);

		public static void MapIsRefreshing(IRefreshViewHandler handler, IRefreshView refreshView)
			=> UpdateIsRefreshing(handler);

		public static void MapContent(IRefreshViewHandler handler, IRefreshView refreshView)
			=> UpdateContent(handler);

		public static void MapRefreshColor(IRefreshViewHandler handler, IRefreshView refreshView)
			=> UpdateRefreshColor(handler);

		public static void MapIsEnabled(IRefreshViewHandler handler, IRefreshView refreshView)
			=> handler.PlatformView?.UpdateIsEnabled(refreshView.IsEnabled);

		static void UpdateIsRefreshing(IRefreshViewHandler handler)
		{
			handler.PlatformView.IsRefreshing = handler.VirtualView.IsRefreshing;
		}

		static void UpdateContent(IRefreshViewHandler handler)
		{
			if (handler.VirtualView is IContentView cv && cv.PresentedContent is IView view)
			{
				handler.PlatformView.UpdateContent(view, handler.MauiContext);
			}
			else
			{
				handler.PlatformView.UpdateContent(handler.VirtualView.Content, handler.MauiContext);
			}

		}

		static void UpdateRefreshColor(IRefreshViewHandler handler)
		{
			var color = handler.VirtualView?.RefreshColor?.ToColor()?.ToPlatform();

			if (color != null)
				handler.PlatformView.RefreshControl.TintColor = color;
		}

		class MauiRefreshViewProxy
		{
			WeakReference<IRefreshView>? _virtualView;

			IRefreshView? VirtualView => _virtualView is not null && _virtualView.TryGetTarget(out var v) ? v : null;

			public void Connect(IRefreshView virtualView, MauiRefreshView platformView)
			{
				_virtualView = new(virtualView);
				platformView.RefreshControl.ValueChanged += OnRefresh;
			}

			public void Disconnect(MauiRefreshView platformView)
			{
				_virtualView = null;
				platformView.RefreshControl.ValueChanged -= OnRefresh;
			}

			void OnRefresh(object? sender, EventArgs e)
			{
				if (VirtualView is IRefreshView virtualView)
					virtualView.IsRefreshing = true;
			}
		}
	}
}
