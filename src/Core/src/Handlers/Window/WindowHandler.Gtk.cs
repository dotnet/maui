using System;

namespace Microsoft.Maui.Handlers
{

	public partial class WindowHandler : ElementHandler<IWindow, Gtk.Window>
	{

		public static void MapTitle(WindowHandler handler, IWindow window) =>
			handler.NativeView.UpdateTitle(window);

		public static void MapContent(WindowHandler handler, IWindow window)
		{
			_ = handler.MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");

			var nativeContent = window.Content.ToNative(handler.MauiContext);

			handler.NativeView.Child = nativeContent;
		}

	}

}