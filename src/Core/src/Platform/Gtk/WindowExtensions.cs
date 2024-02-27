using System;
using Microsoft.Maui.Devices;

namespace Microsoft.Maui.Platform
{
	public static partial class WindowExtensions
	{
		public static void UpdateTitle(this Gtk.Window platformWindow, IWindow window) =>
			platformWindow.Title = window.Title;

		public static IWindow GetWindow(this Gtk.Window platformWindow)
		{
			foreach (var window in MauiGtkApplication.Current.Application.Windows)
			{
				if (window?.Handler?.PlatformView is Gtk.Window win && win == platformWindow)
					return window;
			}

			throw new InvalidOperationException("Window Not Found");
		}

		public static void SetWindow(this Gtk.Window platformWindow, IWindow window, IMauiContext context)
		{
			_ = platformWindow ?? throw new ArgumentNullException(nameof(platformWindow));
			_ = window ?? throw new ArgumentNullException(nameof(window));
			_ = context ?? throw new ArgumentNullException(nameof(context));

			var handler = window.Handler;

			if (handler == null)
				handler = context.Handlers.GetHandler(window.GetType());

			if (handler == null)
				throw new Exception($"Handler not found for window {window}.");

			handler.SetMauiContext(context);

			window.Handler = handler;

			if (handler.VirtualView != window)
				handler.SetVirtualView(window);
		}

		internal static DisplayOrientation GetOrientation(this IWindow? window) =>
			DeviceDisplay.Current.MainDisplayInfo.Orientation;

		public static void UpdateX(this Gtk.Window platformWindow, IWindow window) =>
			platformWindow.UpdatePosition(window);

		public static void UpdateY(this Gtk.Window platformWindow, IWindow window) =>
			platformWindow.UpdatePosition(window);

		public static void UpdatePosition(this Gtk.Window platformWindow, IWindow window)
		{
			var x = (int)window.X;
			var y = (int)window.Y;

			platformWindow.Move(x, y);
		}

		public static void UpdateWidth(this Gtk.Window platformWindow, IWindow window) =>
			platformWindow.UpdateSize(window);

		public static void UpdateHeight(this Gtk.Window platformWindow, IWindow window) =>
			platformWindow.UpdateSize(window);

		public static void UpdateSize(this Gtk.Window platformWindow, IWindow window)
		{
			var width = (int)window.Width;
			var height = (int)window.Height;

			if (width <= 0 || height <= 0)
				return;

			platformWindow.Resize(width, height);
			platformWindow.QueueResize();
		}

		public static float GetDisplayDensity(this Gtk.Window platformWindow) =>
			(float)platformWindow.ScaleFactor;
	}
}