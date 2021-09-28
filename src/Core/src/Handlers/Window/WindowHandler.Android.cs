using System;
using Android.App;

namespace Microsoft.Maui.Handlers
{
	public partial class WindowHandler : ElementHandler<IWindow, Activity>
	{
		public static void MapTitle(WindowHandler handler, IWindow window) { }

		public static void MapContent(WindowHandler handler, IWindow window)
		{
			_ = handler.MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");

			// TODO MAUI
			//var nativeContent = window.Content.ToContainerView(handler.MauiContext);

			// Not sure how to connect these things together
			var navigationRoot = handler.MauiContext.GetNavigationRootManager();
			//_ = window.Content.ToNative(handler.MauiContext);
			navigationRoot.SetContentView(window.Content.ToNative(handler.MauiContext));
			handler.NativeView.SetContentView(navigationRoot.RootView);
		}
	}
}