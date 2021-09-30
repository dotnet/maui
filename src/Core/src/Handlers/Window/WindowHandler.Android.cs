using System;
using Android.App;
using Google.Android.Material.AppBar;

namespace Microsoft.Maui.Handlers
{
	public partial class WindowHandler : ElementHandler<IWindow, Activity>
	{
		public NavigationRootManager? NavigationRootManager { get; set; }

		public static void MapTitle(WindowHandler handler, IWindow window) { }

		public static void MapContent(WindowHandler handler, IWindow window)
		{
			_ = handler.MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");

			// TODO MAUI: TOOLBAR
			handler.NavigationRootManager ??= new NavigationRootManager(handler.MauiContext);

			var navRootManager = handler.NavigationRootManager;

			// TODO MAUI
			//var nativeContent = window.Content.ToContainerView(handler.MauiContext);

			// Not sure how to connect these things together
			//_ = window.Content.ToNative(handler.MauiContext);
			handler.NavigationRootManager.SetContentView(window.Content);
			handler.NativeView.SetContentView(navRootManager.RootView);
		}
	}
}