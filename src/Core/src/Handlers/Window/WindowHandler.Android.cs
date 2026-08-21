using System;
using Android.App;
using Android.Content.Res;
using Android.Views;
using AndroidX.Core.Graphics;
using AndroidX.Core.View;
using AndroidX.Window.Layout;
using Google.Android.Material.AppBar;
using Microsoft.Maui.Platform;
using AColor = Android.Graphics.Color;
using AView = Android.Views.View;

namespace Microsoft.Maui.Handlers
{
	public partial class WindowHandler : ElementHandler<IWindow, Activity>
	{
		NavigationRootManager? _rootManager;
		bool _disconnecting;
		int _rootManagerLifecycle;

		protected override void ConnectHandler(Activity platformView)
		{
			_disconnecting = false;
			_rootManagerLifecycle++;
			base.ConnectHandler(platformView);
			if (OperatingSystem.IsAndroidVersionAtLeast(30))
			{
				//Edge to Edge enabled for Android API 30+
				PlatformView.Window.ConfigureTranslucentSystemBars(PlatformView);
			}
			UpdateVirtualViewFrame(platformView);
		}

		public static void MapTitle(IWindowHandler handler, IWindow window) =>
			handler.PlatformView.UpdateTitle(window);

		public static void MapContent(IWindowHandler handler, IWindow window)
		{
			_ = handler.MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");

			if (handler is WindowHandler windowHandler && windowHandler._disconnecting)
				return;

			View? publishedRoot = null;
			ConnectRootViewFromContent(handler, window, (rootManager, rootView) =>
			{
				if (rootView is null)
					return;

				if (handler is WindowHandler currentHandler && currentHandler._disconnecting)
				{
					rootManager.Disconnect();
					return;
				}

				var activity = ((IElementHandler)handler).PlatformView as Activity;
				if (activity is null || activity.IsDestroyed || activity.IsFinishing)
				{
					rootManager.Disconnect();
					return;
				}

				activity.SetContentView(rootView);
				publishedRoot = rootView;
			}, (_, rootView) =>
			{
				if (rootView is not null && ReferenceEquals(publishedRoot, rootView))
				{
					rootView.RemoveFromParent();
					publishedRoot = null;
				}
			});
		}

		public static void MapX(IWindowHandler handler, IWindow view) =>
			handler.PlatformView?.UpdateX(view);

		public static void MapY(IWindowHandler handler, IWindow view) =>
			handler.PlatformView?.UpdateY(view);

		public static void MapWidth(IWindowHandler handler, IWindow view) =>
			handler.PlatformView?.UpdateWidth(view);

		public static void MapHeight(IWindowHandler handler, IWindow view) =>
			handler.PlatformView?.UpdateHeight(view);

		public static void MapToolbar(IWindowHandler handler, IWindow view)
		{
			if (view is IToolbarElement tb)
				ViewHandler.MapToolbar(handler, tb);
		}

		public static void MapRequestDisplayDensity(IWindowHandler handler, IWindow window, object? args)
		{
			if (args is DisplayDensityRequest request)
				request.SetResult(handler.PlatformView.GetDisplayDensity());
		}

		private protected override void OnConnectHandler(object platformView)
		{
			base.OnConnectHandler(platformView);

			var rootManager = MauiContext!.GetNavigationRootManager();
			if (_rootManager is not null && !ReferenceEquals(_rootManager, rootManager))
				_rootManager.RootViewChanged -= OnRootViewChanged;

			_rootManager = rootManager;
			_rootManager.RootViewChanged -= OnRootViewChanged;
			_rootManager.RootViewChanged += OnRootViewChanged;
		}

		private protected override void OnDisconnectHandler(object platformView)
		{
			base.OnDisconnectHandler(platformView);
			_disconnecting = true;
			var lifecycle = ++_rootManagerLifecycle;

			if (_rootManager is { } rootManager)
			{
				rootManager.Disconnect((_, _) => CompleteRootManagerDisconnect(rootManager, lifecycle));
			}
		}

		void OnRootViewChanged(object? sender, EventArgs e)
		{
			var virtualView = ((IElementHandler)this).VirtualView as IWindow;
			if (virtualView is null)
				return;

			if (_disconnecting)
			{
				if (virtualView.VisualDiagnosticsOverlay?.IsPlatformViewInitialized == true)
					virtualView.VisualDiagnosticsOverlay.Deinitialize();

				return;
			}

			if (virtualView.VisualDiagnosticsOverlay != null && _rootManager?.RootView is ViewGroup)
			{
				if (virtualView.VisualDiagnosticsOverlay.IsPlatformViewInitialized)
					virtualView.VisualDiagnosticsOverlay.Deinitialize();

				virtualView.VisualDiagnosticsOverlay.Initialize();
			}
		}

		void CompleteRootManagerDisconnect(NavigationRootManager rootManager, int lifecycle)
		{
			if (lifecycle != _rootManagerLifecycle)
				return;

			rootManager.RootViewChanged -= OnRootViewChanged;

			// The MauiCoordinatorLayout will automatically unregister from the static registry
			// when it's detached from the window, but we can ensure cleanup here as well.
			if (ReferenceEquals(_rootManager, rootManager))
				_rootManager = null;
		}

		// This is here to try and ensure symmetry with disconnect code between test handler
		// and the real handler
		internal static void DisconnectHandler(NavigationRootManager? navigationRootManager)
		{
			navigationRootManager?.Disconnect();
		}

		internal static void ConnectRootViewFromContent(
			IWindowHandler handler,
			IWindow window,
			Action<NavigationRootManager, View?> rootPrepared,
			Action<NavigationRootManager, View?> rootDiscarded)
		{
			_ = handler.MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");

			var rootManager = handler.MauiContext.GetNavigationRootManager();
			rootManager.Connect(
				window.Content,
				rootPrepared: rootView => rootPrepared(rootManager, rootView),
				rootDiscarded: rootView => rootDiscarded(rootManager, rootView));
		}

		void UpdateVirtualViewFrame(Activity activity)
		{
			var frame = activity.GetWindowFrame();
			VirtualView.FrameChanged(frame);
		}
	}
}