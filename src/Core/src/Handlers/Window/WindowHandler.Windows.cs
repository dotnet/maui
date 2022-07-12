using System;
using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui.Handlers
{
	public partial class WindowHandler : ElementHandler<IWindow, UI.Xaml.Window>
	{
		protected override void ConnectHandler(UI.Xaml.Window platformView)
		{
			base.ConnectHandler(platformView);

			if (platformView.Content is null)
				platformView.Content = new WindowRootViewContainer();
		}

		protected override void DisconnectHandler(UI.Xaml.Window platformView)
		{
			MauiContext
				?.GetNavigationRootManager()
				?.Disconnect();

			if (platformView.Content is WindowRootViewContainer container)
			{
				container.Children.Clear();
				platformView.Content = null;
			}

			base.DisconnectHandler(platformView);
		}

		public static void MapTitle(IWindowHandler handler, IWindow window) =>
			handler.PlatformView?.UpdateTitle(window);

		public static void MapContent(IWindowHandler handler, IWindow window)
		{
			_ = handler.MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");

			var windowManager = handler.MauiContext.GetNavigationRootManager();
			var previousRootView = windowManager.RootView;

			windowManager.Disconnect();
			windowManager.Connect(handler.VirtualView.Content.ToPlatform(handler.MauiContext));

			if (handler.PlatformView.Content is WindowRootViewContainer container)
			{
				if (previousRootView != null && previousRootView != windowManager.RootView)
					container.RemovePage(previousRootView);

				container.AddPage(windowManager.RootView);
			}

			if (window.VisualDiagnosticsOverlay != null)
				window.VisualDiagnosticsOverlay.Initialize();
		}

		public static void MapToolbar(IWindowHandler handler, IWindow view)
		{
			if (view is IToolbarElement tb)
				ViewHandler.MapToolbar(handler, tb);
		}

		public static void MapMenuBar(IWindowHandler handler, IWindow view)
		{
			if (view is IMenuBarElement mb)
			{
				_ = handler.MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");
				var windowManager = handler.MauiContext.GetNavigationRootManager();
				windowManager.SetMenuBar(mb.MenuBar?.ToPlatform(handler.MauiContext!) as MenuBar);
			}
		}

		public static void MapFlowDirection(IWindowHandler handler, IWindow view)
		{
			var WindowHandle = handler.PlatformView.GetWindowHandle();

			// Retrieve current extended style
			var extended_style = PlatformMethods.GetWindowLongPtr(WindowHandle, PlatformMethods.WindowLongFlags.GWL_EXSTYLE);
			long updated_style;
			if (view.FlowDirection == FlowDirection.RightToLeft)
				updated_style = extended_style | (long)PlatformMethods.ExtendedWindowStyles.WS_EX_LAYOUTRTL;
			else
				updated_style = extended_style & ~((long)PlatformMethods.ExtendedWindowStyles.WS_EX_LAYOUTRTL);

			if (updated_style != extended_style)
				PlatformMethods.SetWindowLongPtr(WindowHandle, PlatformMethods.WindowLongFlags.GWL_EXSTYLE, updated_style);
		}

		public static void MapRequestDisplayDensity(IWindowHandler handler, IWindow window, object? args)
		{
			if (args is DisplayDensityRequest request)
				request.SetResult(handler.PlatformView.GetDisplayDensity());
		}
	}
}