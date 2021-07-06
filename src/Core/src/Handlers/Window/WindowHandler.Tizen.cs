using System;
using ElmSharp;

namespace Microsoft.Maui.Handlers
{
	public partial class WindowHandler : ElementHandler<IWindow, Window>
	{
		public static void MapTitle(WindowHandler handler, IWindow window) { }

		public static void MapContent(WindowHandler handler, IWindow window)
		{
			_ = handler.MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");
			_ = handler.MauiContext.Context ?? throw new InvalidOperationException($"{nameof(CoreUIAppContext)} should have been set by base class.");

			var nativeContent = window.Content.ToNative(handler.MauiContext);
			handler.MauiContext.Context.SetContent(nativeContent);
		}
	}
}