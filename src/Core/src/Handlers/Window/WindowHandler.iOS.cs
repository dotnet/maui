using System;
using Foundation;
using UIKit;

namespace Microsoft.Maui.Handlers
{
	public partial class WindowHandler : ElementHandler<IWindow, UIWindow>
	{
		readonly WindowProxy _windowProxy = new();
		readonly FrameObserverProxy _frameObserverProxy = new();

		protected override void ConnectHandler(UIWindow platformView)
		{
			base.ConnectHandler(platformView);

			_frameObserverProxy.Connect(VirtualView, platformView);

			// For newer Mac Catalyst versions, we want to wait until we get effective window dimensions from the platform.
			if (OperatingSystem.IsMacCatalystVersionAtLeast(16))
			{
				_windowProxy.Connect(VirtualView, platformView);
			}
			else
			{
				UpdateVirtualViewFrame(platformView);
			}
		}

		protected override void DisconnectHandler(UIWindow platformView)
		{
			if (OperatingSystem.IsMacCatalystVersionAtLeast(16))
			{
				_windowProxy.Disconnect();
				_frameObserverProxy.Disconnect(platformView);
			}

			base.DisconnectHandler(platformView);
		}

		public static void MapTitle(IWindowHandler handler, IWindow window) =>
			handler.PlatformView.UpdateTitle(window);

#if MACCATALYST
		internal static void MapTitleBar(IWindowHandler handler, IWindow window)
		{
			if (handler.PlatformView.RootViewController is WindowViewController controller && handler?.MauiContext is IMauiContext mauiContext)
			{
				controller.SetUpTitleBar(window, mauiContext);
			}
		}
#endif

		public static void MapContent(IWindowHandler handler, IWindow window)
		{
			_ = handler.MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");

			var nativeContent = window.Content.ToUIViewController(handler.MauiContext);

			var rootViewController = handler.PlatformView.RootViewController;

#if MACCATALYST
			// If we are in Catalyst, use the WindowViewController so we can handle the TitleBar
			handler.PlatformView.RootViewController = new WindowViewController(nativeContent, window, handler.MauiContext);
#else
			handler.PlatformView.RootViewController = nativeContent;
#endif

			window.VisualDiagnosticsOverlay?.Initialize();
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
			{
				request.SetResult(handler.PlatformView.GetDisplayDensity());
			}
		}

		void UpdateVirtualViewFrame(UIWindow window)
		{
			VirtualView.FrameChanged(window.Bounds.ToRectangle());
		}

		class WindowProxy
		{
			WeakReference<IWindow>? _virtualView;

			IWindow? VirtualView => _virtualView is not null && _virtualView.TryGetTarget(out var v) ? v : null;
			IDisposable? _effectiveGeometryObserver;

			public void Connect(IWindow virtualView, UIWindow platformView)
			{
				_virtualView = new(virtualView);

				// https://developer.apple.com/documentation/uikit/uiwindowscene/effectivegeometry?language=objc#Discussion mentions:
				// > This property is key-value observing (KVO) compliant. Observing effectiveGeometry is the recommended way
				// > to receive notifications of changes to the window scene’s geometry. These changes can occur because of
				// > user interaction or as a result of the system resolving a geometry request.
				_effectiveGeometryObserver = platformView.WindowScene?.AddObserver("effectiveGeometry", NSKeyValueObservingOptions.OldNew, HandleEffectiveGeometryObserved);
			}

			public void Disconnect()
			{
				_effectiveGeometryObserver?.Dispose();
			}

			void HandleEffectiveGeometryObserved(NSObservedChange obj)
			{
				if (obj is not null && VirtualView is IWindow virtualView && obj.NewValue is UIWindowSceneGeometry newGeometry)
				{
					var newRectangle = newGeometry.SystemFrame.ToRectangle();

					if (double.IsNaN(newRectangle.X) || double.IsNaN(newRectangle.Y) || double.IsNaN(newRectangle.Width) || double.IsNaN(newRectangle.Height))
					{
						return;
					}

					virtualView.FrameChanged(newRectangle);
				}
			}
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