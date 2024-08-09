using System;
using UIKit;

namespace Microsoft.Maui.Handlers
{
	public partial class WindowHandler : ElementHandler<IWindow, UIWindow>
	{
		readonly FrameObserverProxy _proxy = new();

		protected override void ConnectHandler(UIWindow platformView)
		{
			base.ConnectHandler(platformView);
			
			_proxy.Connect(VirtualView, platformView);
			_proxy.Update();
		}

		protected override void DisconnectHandler(UIWindow platformView)
		{
			_proxy.Disconnect(platformView);

			base.DisconnectHandler(platformView);
		}

		public static void MapTitle(IWindowHandler handler, IWindow window) =>
			handler.PlatformView.UpdateTitle(window);

		public static void MapContent(IWindowHandler handler, IWindow window)
		{
			_ = handler.MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");

			var nativeContent = window.Content.ToUIViewController(handler.MauiContext);

			handler.PlatformView.RootViewController = nativeContent;

			if (window.VisualDiagnosticsOverlay != null)
				window.VisualDiagnosticsOverlay.Initialize();
		}

		public static void MapX(IWindowHandler handler, IWindow view) =>
			handler.PlatformView?.UpdateX(view);

		public static void MapY(IWindowHandler handler, IWindow view) =>
			handler.PlatformView?.UpdateY(view);

		public static void MapWidth(IWindowHandler handler, IWindow view) =>
			handler.PlatformView?.UpdateWidth(view);

		public static void MapHeight(IWindowHandler handler, IWindow view) =>
			handler.PlatformView?.UpdateHeight(view);

		public static void MapMaximumWidth(IWindowHandler handler, IWindow view) =>
			handler.PlatformView?.UpdateMaximumWidth(view);

		public static void MapMaximumHeight(IWindowHandler handler, IWindow view) =>
			handler.PlatformView?.UpdateMaximumHeight(view);

		public static void MapMinimumWidth(IWindowHandler handler, IWindow view) =>
			handler.PlatformView?.UpdateMinimumWidth(view);

		public static void MapMinimumHeight(IWindowHandler handler, IWindow view) =>
			handler.PlatformView?.UpdateMinimumHeight(view);

		public static void MapMenuBar(IWindowHandler handler, IWindow view)
		{
			if (!(OperatingSystem.IsIOSVersionAtLeast(13) || OperatingSystem.IsMacCatalystVersionAtLeast(13)))
				return;

			if (MauiUIApplicationDelegate.Current != null &&
				view is IMenuBarElement mb)
			{
				if (MauiUIApplicationDelegate.Current.MenuBuilder == null)
				{
					UIMenuSystem
						.MainSystem
						.SetNeedsRebuild();
				}
				else
				{
					// The handlers that are part of MenuBar
					// are only relevant while the menu is being built
					// because you can only build a menu while the
					// `AppDelegate.BuildMenu` override is running
					mb.MenuBar?.Handler?.DisconnectHandler();
					mb.MenuBar?
						.ToHandler(handler.MauiContext!)?
						.DisconnectHandler();
				}
			}
		}

		public static void MapRequestDisplayDensity(IWindowHandler handler, IWindow window, object? args)
		{
			if (args is DisplayDensityRequest request)
				request.SetResult(handler.PlatformView.GetDisplayDensity());
		}

		class FrameObserverProxy
		{
			WeakReference<IWindow>? _virtualView;
			WeakReference<UIWindow>? _platformView;

			IDisposable? _frameObserver;

			IWindow? VirtualView => _virtualView is not null && _virtualView.TryGetTarget(out var v) ? v : null;

			UIWindow? PlatformView => _platformView is not null && _platformView.TryGetTarget(out var v) ? v : null;

			public void Connect(IWindow virtualView, UIWindow platformView)
			{
				_virtualView = new(virtualView);
				_platformView = new(platformView);

				_frameObserver = platformView.AddObserver("frame", Foundation.NSKeyValueObservingOptions.New, FrameAction);
			}

			public void Disconnect(UIWindow platformView)
			{
				_virtualView = null;
				_platformView = null;

				_frameObserver?.Dispose();
			}

			public void Update()
			{
				if (VirtualView is IWindow virtualView && PlatformView is UIWindow platformView)
				{
					virtualView.FrameChanged(platformView.Frame.ToRectangle());
				}
			}

			void FrameAction(Foundation.NSObservedChange obj) => Update();
		}
	}
}