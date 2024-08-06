using System;
using Foundation;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Handlers
{
	public partial class WindowHandler : ElementHandler<IWindow, UIWindow>
	{
		private IDisposable? effectiveGeometryObserver;

		protected override void ConnectHandler(UIWindow platformView)
		{
			base.ConnectHandler(platformView);

			UpdateVirtualViewFrame(platformView);

			effectiveGeometryObserver = platformView.WindowScene?.AddObserver("effectiveGeometry", NSKeyValueObservingOptions.OldNew, HandleEffectiveGeometryObserved);
		}

#pragma warning disable RS0016 // Add public types and members to the declared API
		protected override void DisconnectHandler(UIWindow platformView)
#pragma warning restore RS0016 // Add public types and members to the declared API
		{
			base.DisconnectHandler(platformView);

			effectiveGeometryObserver?.Dispose();
		}

		void HandleEffectiveGeometryObserved(NSObservedChange obj)
		{
			if (obj is not null && obj.OldValue is UIWindowSceneGeometry oldGeometry && obj.NewValue is UIWindowSceneGeometry newGeometry)
			{
				Console.WriteLine($" oldValue={oldGeometry.SystemFrame}, newValue={newGeometry.SystemFrame}");
				VirtualView.FrameChanged(newGeometry.SystemFrame.ToRectangle());
			}
		}

		public static void MapTitle(IWindowHandler handler, IWindow window) =>
			handler.PlatformView.UpdateTitle(window);

		public static void MapContent(IWindowHandler handler, IWindow window)
		{
			_ = handler.MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");

			var nativeContent = window.Content.ToUIViewController(handler.MauiContext);

			handler.PlatformView.RootViewController = nativeContent;

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
				request.SetResult(handler.PlatformView.GetDisplayDensity());
		}

		void UpdateVirtualViewFrame(UIWindow window)
		{
			VirtualView.FrameChanged(window.Bounds.ToRectangle());
		}
	}
}