using System;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Graphics;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml.Controls;
using Windows.Graphics;

namespace Microsoft.Maui.Handlers
{
	public partial class WindowHandler : ElementHandler<IWindow, UI.Xaml.Window>
	{
		protected override void ConnectHandler(UI.Xaml.Window platformView)
		{
			base.ConnectHandler(platformView);

			if (platformView.Content is null)
				platformView.Content = new WindowRootViewContainer();

			// update the platform window with the user size/position
			platformView.UpdatePosition(VirtualView);
			platformView.UpdateSize(VirtualView);

			var appWindow = platformView.GetAppWindow();
			if (appWindow is not null)
			{
				// then pass the actual size back to the user
				UpdateVirtualViewFrame(appWindow);

				// THEN attach the event to reduce churn
				appWindow.Changed += OnWindowChanged;
			}

			var windowRootContentManager = MauiContext
				?.GetNavigationRootManager();

			if (windowRootContentManager is not null)
			{
				windowRootContentManager.OnApplyTemplateFinished += WindowRootContentManagerOnApplyTemplateFinished;
			}
		}

		void WindowRootContentManagerOnApplyTemplateFinished(object? sender, EventArgs e)
		{
			UpdateValue(nameof(IWindow.TitleBarDragRectangles));
		}

		protected override void DisconnectHandler(UI.Xaml.Window platformView)
		{
			var windowRootContentManager = MauiContext
				?.GetNavigationRootManager();

			if (windowRootContentManager is not null)
			{
				windowRootContentManager.OnApplyTemplateFinished -= WindowRootContentManagerOnApplyTemplateFinished;
				windowRootContentManager.SetTitleBar(null, null);
				windowRootContentManager.Disconnect();
			}

			if (platformView.Content is WindowRootViewContainer container)
			{
				container.CachedChildren.Clear();
				platformView.Content = null;
			}

			var appWindow = platformView.GetAppWindow();
			if (appWindow is not null)
			{
				appWindow.Changed -= OnWindowChanged;
			}

			base.DisconnectHandler(platformView);
		}

		public static void MapTitle(IWindowHandler handler, IWindow window) =>
			handler.PlatformView?.UpdateTitle(window);

		public static void MapContent(IWindowHandler handler, IWindow window)
		{
			_ = handler.MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");

			var windowManager = handler.MauiContext.GetNavigationRootManager();
			windowManager.Connect(handler);

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

		public static void MapIsMinimizable(IWindowHandler handler, IWindow window) =>
			handler.PlatformView?.UpdateIsMinimizable(window);

		public static void MapIsMaximizable(IWindowHandler handler, IWindow window) =>
			handler.PlatformView?.UpdateIsMaximizable(window);

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

		internal static void MapTitleBarDragRectangles(IWindowHandler handler, IWindow window)
		{
			if (!AppWindowTitleBar.IsCustomizationSupported())
				return;

			var titleBar = handler.PlatformView.GetAppWindow()?.TitleBar;
			if (titleBar is null)
				return;

			var titleBarRects = window.TitleBarDragRectangles;

			if (titleBarRects is null)
			{
				titleBar.SetDragRectangles(null);
			}
			else
			{
				var density = handler.PlatformView.GetDisplayDensity();
				RectInt32[] dragRects = new RectInt32[titleBarRects.Length];
				for (var i = 0; i < titleBarRects.Length; i++)
				{
					Rect rect = titleBarRects[i];
					dragRects[i] = new RectInt32(
					(int)(rect.X * density),
					(int)(rect.Y * density),
					(int)(rect.Width * density),
					(int)(rect.Height * density));
				}

				titleBar.SetDragRectangles(dragRects);
			}
		}

		internal static void MapTitleBar(IWindowHandler handler, IWindow window)
		{
			handler
				.PlatformView
				.UpdateTitleBar(window, handler.MauiContext);
		}

		void OnWindowChanged(AppWindow sender, AppWindowChangedEventArgs args)
		{
			if (!args.DidSizeChange && !args.DidPositionChange)
				return;

			UpdateVirtualViewFrame(sender);
		}

		void UpdateVirtualViewFrame(AppWindow appWindow)
		{
			var size = appWindow.Size;
			var pos = appWindow.Position;

			var density = PlatformView.GetDisplayDensity();

			VirtualView.FrameChanged(new Rect(
				pos.X / density, pos.Y / density,
				size.Width / density, size.Height / density));
		}
	}
}
