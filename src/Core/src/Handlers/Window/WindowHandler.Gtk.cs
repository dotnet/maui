using System;

namespace Microsoft.Maui.Handlers
{

	public partial class WindowHandler : ElementHandler<IWindow, Gtk.Window>
	{

		protected override void ConnectHandler(Gtk.Window platformView)
		{
			base.ConnectHandler(platformView);

			// update the platform window with the user size/position
			platformView.UpdatePosition(VirtualView);
			platformView.UpdateSize(VirtualView);
		}

		public static void MapTitle(IWindowHandler handler, IWindow window) =>
			handler.PlatformView.UpdateTitle(window);

		public static void MapContent(IWindowHandler handler, IWindow window)
		{
			_ = handler.MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");

			var nativeContent = window.Content.ToPlatform(handler.MauiContext);

			handler.PlatformView.Child = nativeContent;
		}

		public static void MapRequestDisplayDensity(IWindowHandler handler, IWindow window, object? args)
		{
			if (args is DisplayDensityRequest request)
				request.SetResult(handler.PlatformView.GetDisplayDensity());
		}

		public static void MapX(IWindowHandler handler, IWindow view) =>
			handler.PlatformView?.UpdateX(view);

		public static void MapY(IWindowHandler handler, IWindow view) =>
			handler.PlatformView?.UpdateY(view);

		public static void MapWidth(IWindowHandler handler, IWindow view) =>
			handler.PlatformView?.UpdateWidth(view);

		public static void MapHeight(IWindowHandler handler, IWindow view) =>
			handler.PlatformView?.UpdateHeight(view);

	}

}